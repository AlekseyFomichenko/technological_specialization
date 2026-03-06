using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Server.Data.Abstracts;
using Server.Models;
using Server.Options;
using Server.Services.Abstracts;
using Shared.DTO;
using Shared.Models;

namespace Server.Services
{
    internal class FileTransferService : IFileTransferService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        private readonly IFileStorage _fileStorage;
        private readonly IFileMetadataRepository _fileMetadataRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMessageDelivery _messageDelivery;
        private readonly FileStorageOptions _options;
        private readonly ILogger<FileTransferService> _logger;
        private readonly ConcurrentDictionary<Guid, ActiveReceive> _receives = new();

        public FileTransferService(
            IFileStorage fileStorage,
            IFileMetadataRepository fileMetadataRepository,
            IUserRepository userRepository,
            IMessageDelivery messageDelivery,
            IOptions<FileStorageOptions> options,
            ILogger<FileTransferService> logger)
        {
            _fileStorage = fileStorage;
            _fileMetadataRepository = fileMetadataRepository;
            _userRepository = userRepository;
            _messageDelivery = messageDelivery;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<StartFileResult> StartReceivingAsync(Guid connectionId, string senderLogin, FileStartPayload payload, CancellationToken cancellationToken = default)
        {
            long maxBytes = _options.MaxFileSizeMb * 1024L * 1024;
            if (payload.FileSize > maxBytes)
                return StartFileResult.Fail(ErrorCodes.FileTooLarge, "File size exceeds limit.");

            var extension = Path.GetExtension(payload.FileName);
            if (string.IsNullOrEmpty(extension) || !Array.Exists(_options.AllowedExtensions ?? Array.Empty<string>(), e => string.Equals(e, extension, StringComparison.OrdinalIgnoreCase)))
                return StartFileResult.Fail(ErrorCodes.ExtensionNotAllowed, "File extension not allowed.");

            User? receiver = await _userRepository.GetByLoginAsync(payload.ReceiverLogin, cancellationToken).ConfigureAwait(false);
            if (receiver is null)
                return StartFileResult.Fail(ErrorCodes.ReceiverNotFound, "Receiver not found.");

            (Stream writeStream, string relativePath) = await _fileStorage.CreateWriteStreamAsync(payload.FileName, cancellationToken).ConfigureAwait(false);

            var receive = new ActiveReceive
            {
                Stream = writeStream,
                RelativePath = relativePath,
                SenderLogin = senderLogin,
                ReceiverLogin = payload.ReceiverLogin,
                FileName = payload.FileName,
                ExpectedFileSize = payload.FileSize,
                ReceivedBytes = 0
            };

            if (!_receives.TryAdd(connectionId, receive))
            {
                await writeStream.DisposeAsync().ConfigureAwait(false);
                await _fileStorage.DeleteAsync(relativePath, cancellationToken).ConfigureAwait(false);
                return StartFileResult.Fail(ErrorCodes.NoActiveReceive, "Already receiving a file on this connection.");
            }

            return StartFileResult.Ok();
        }

        public async Task<WriteChunkResult> WriteChunkAsync(Guid connectionId, ReadOnlyMemory<byte> chunk, CancellationToken cancellationToken = default)
        {
            if (!_receives.TryGetValue(connectionId, out ActiveReceive? receive))
                return WriteChunkResult.Fail(ErrorCodes.NoActiveReceive, "No active file receive.");

            long maxBytes = _options.MaxFileSizeMb * 1024L * 1024;
            receive.ReceivedBytes += chunk.Length;
            if (receive.ReceivedBytes > maxBytes)
            {
                _receives.TryRemove(connectionId, out _);
                await CleanupReceiveAsync(receive, cancellationToken).ConfigureAwait(false);
                return WriteChunkResult.Fail(ErrorCodes.SizeExceeded, "File size exceeded during transfer.");
            }

            await receive.Stream.WriteAsync(chunk, cancellationToken).ConfigureAwait(false);
            return WriteChunkResult.Ok();
        }

        public async Task<EndFileResult> EndReceivingAsync(Guid connectionId, CancellationToken cancellationToken = default)
        {
            if (!_receives.TryRemove(connectionId, out ActiveReceive? receive))
                return EndFileResult.Fail(ErrorCodes.NoActiveReceive, "No active file receive.");

            await receive.Stream.DisposeAsync().ConfigureAwait(false);

            var fileId = Guid.NewGuid();
            var metadata = new FileMetadata
            {
                Id = fileId,
                SenderLogin = receive.SenderLogin,
                ReceiverLogin = receive.ReceiverLogin,
                FileName = receive.FileName,
                FilePath = receive.RelativePath,
                FileSize = receive.ReceivedBytes,
                CreatedAt = DateTime.UtcNow
            };
            await _fileMetadataRepository.AddAsync(metadata, cancellationToken).ConfigureAwait(false);

            var notification = new FileAvailablePayload
            {
                FileId = fileId,
                SenderLogin = receive.SenderLogin,
                FileName = receive.FileName,
                FileSize = receive.ReceivedBytes
            };
            byte[] notificationBytes = JsonSerializer.SerializeToUtf8Bytes(notification, JsonOptions);
            await _messageDelivery.SendToUserAsync(receive.ReceiverLogin, MessageType.FileStart, notificationBytes.AsMemory(), cancellationToken).ConfigureAwait(false);

            const int chunkSize = 65536;
            await using (Stream readStream = await _fileStorage.OpenReadAsync(receive.RelativePath, cancellationToken).ConfigureAwait(false))
            {
                var buffer = new byte[chunkSize];
                int read;
                while ((read = await readStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await _messageDelivery.SendToUserAsync(receive.ReceiverLogin, MessageType.FileChunk, buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                }
            }

            var isSent = await _messageDelivery.SendToUserAsync(receive.ReceiverLogin, MessageType.FileEnd, Array.Empty<byte>(), cancellationToken).ConfigureAwait(false);
            if (isSent)
            {
                bool isUpdate = await _fileMetadataRepository.UpdateDeliveredAsync(fileId, cancellationToken).ConfigureAwait(false);
                if (isUpdate)
                    await _fileStorage.DeleteAsync(receive.RelativePath, cancellationToken).ConfigureAwait(false);
            }

            var ackPayload = new AckPayload { Success = true, Id = fileId };
            byte[] ackBytes = JsonSerializer.SerializeToUtf8Bytes(ackPayload, JsonOptions);
            await _messageDelivery.SendToUserAsync(receive.SenderLogin, MessageType.Ack, ackBytes.AsMemory(), cancellationToken).ConfigureAwait(false);

            return EndFileResult.Ok(fileId);
        }

        public async Task DeliverPendingFilesForUserAsync(string login, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<FileMetadata> undelivered = await _fileMetadataRepository.GetUndeliveredForUserAsync(login, cancellationToken).ConfigureAwait(false);
            const int chunkSize = 65536;
            foreach (FileMetadata metadata in undelivered)
            {
                var fileStartPayload = new FileAvailablePayload
                {
                    FileId = metadata.Id,
                    SenderLogin = metadata.SenderLogin,
                    FileName = metadata.FileName,
                    FileSize = metadata.FileSize
                };
                byte[] fileStartBytes = JsonSerializer.SerializeToUtf8Bytes(fileStartPayload, JsonOptions);
                bool delivered = await _messageDelivery.SendToUserAsync(login, MessageType.FileStart, fileStartBytes.AsMemory(), cancellationToken).ConfigureAwait(false);
                if (!delivered)
                    continue;
                try
                {
                    await using (Stream readStream = await _fileStorage.OpenReadAsync(metadata.FilePath, cancellationToken).ConfigureAwait(false))
                    {
                        var buffer = new byte[chunkSize];
                        int read;
                        while ((read = await readStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                        {
                            await _messageDelivery.SendToUserAsync(login, MessageType.FileChunk, buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                        }
                    }
                    var isSent = await _messageDelivery.SendToUserAsync(login, MessageType.FileEnd, Array.Empty<byte>(), cancellationToken).ConfigureAwait(false);
                    if (isSent)
                    {
                        bool isUpdate = await _fileMetadataRepository.UpdateDeliveredAsync(metadata.Id, cancellationToken).ConfigureAwait(false);
                        if (isUpdate)
                            await _fileStorage.DeleteAsync(metadata.FilePath, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error delivering pending file {FileId} to user {Login}", metadata.Id, login);
                }
            }
        }

        public async Task CancelReceivingAsync(Guid connectionId, CancellationToken cancellationToken = default)
        {
            if (_receives.TryRemove(connectionId, out ActiveReceive? receive))
                await CleanupReceiveAsync(receive, cancellationToken).ConfigureAwait(false);
        }

        private async Task CleanupReceiveAsync(ActiveReceive receive, CancellationToken cancellationToken)
        {
            try
            {
                await receive.Stream.DisposeAsync().ConfigureAwait(false);
                await _fileStorage.DeleteAsync(receive.RelativePath, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error cleaning up file receive");
            }
        }

        private sealed class ActiveReceive
        {
            public required Stream Stream { get; init; }
            public required string RelativePath { get; init; }
            public required string SenderLogin { get; init; }
            public required string ReceiverLogin { get; init; }
            public required string FileName { get; init; }
            public required long ExpectedFileSize { get; init; }
            public long ReceivedBytes { get; set; }
        }
    }
}
