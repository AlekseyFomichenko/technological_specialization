using System.Text.Json;
using Client.Options;
using Client.Protocol;
using Shared.DTO;
using Shared.Models;
using Shared.Protocol;

namespace Client.Services
{
    public sealed class FileClient
    {
        private readonly PacketWriter _writer;
        private readonly ClientOptions _options;
        private readonly PendingResponse _pending;
        private readonly SessionContext _sessionContext;
        private readonly object _receiveLock = new();
        private FileStream? _receiveStream;
        private long _receiveTotal;
        private long _receiveWritten;
        private string? _receiveFilePath;
        private string? _receiveFileName;
        private Guid _receiveSenderId;

        public event EventHandler<(string FileName, Guid SenderId)>? FileReceiveStarted;
        public event EventHandler<(string FileName, string SavedPath)>? FileReceiveCompleted;

        public FileClient(PacketWriter writer, ClientOptions options, PendingResponse pending, SessionContext sessionContext)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _pending = pending ?? throw new ArgumentNullException(nameof(pending));
            _sessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));
        }

        public async Task<SendFileResult> SendFileAsync(string filePath, Guid receiverId, CancellationToken cancellationToken = default)
        {
            var token = _sessionContext.Token;
            if (string.IsNullOrEmpty(token))
                return SendFileResult.Fail("NotAuthenticated", "Not logged in.");

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
                return SendFileResult.Fail("NotFound", "File not found.");
            if (fileInfo.Length > ClientProtocolConstants.MaxFileSizeBytes)
                return SendFileResult.Fail("FileTooLarge", "File exceeds 50 MB limit.");

            var fileName = Path.GetFileName(filePath);
            var startPayload = new FileStartPayload
            {
                Token = token,
                ReceiverId = receiverId,
                FileName = fileName,
                FileSize = fileInfo.Length
            };
            var startBytes = JsonSerializer.SerializeToUtf8Bytes(startPayload, ClientProtocolConstants.JsonOptions);
            _pending.SetPending();
            await _writer.WritePacketAsync(MessageType.FileStart, startBytes, cancellationToken).ConfigureAwait(false);

            var chunkSize = _options.FileChunkSizeBytes;
            await using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, chunkSize, useAsync: true))
            {
                var buffer = new byte[chunkSize];
                int read;
                while ((read = await fs.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await _writer.WritePacketAsync(MessageType.FileChunk, buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                }
            }

            var endPayload = new FileEndPayload();
            var endBytes = JsonSerializer.SerializeToUtf8Bytes(endPayload, ClientProtocolConstants.JsonOptions);
            await _writer.WritePacketAsync(MessageType.FileEnd, endBytes, cancellationToken).ConfigureAwait(false);

            var result = await _pending.WaitAsync(cancellationToken).ConfigureAwait(false);
            return result.Success ? SendFileResult.Ok() : SendFileResult.Fail(result.ErrorCode ?? "Error", result.ErrorMessage ?? "Unknown");
        }

        public void BeginReceive(FileAvailablePayload payload)
        {
            lock (_receiveLock)
            {
                if (_receiveStream is not null)
                {
                    _receiveStream.Dispose();
                    _receiveStream = null;
                }

                var dir = _options.DownloadsPath;
                Directory.CreateDirectory(dir);
                var safeName = Path.GetFileName(payload.FileName);
                if (string.IsNullOrEmpty(safeName))
                    safeName = payload.FileId.ToString();
                var path = Path.Combine(dir, safeName);
                _receiveFilePath = path;
                _receiveFileName = safeName;
                _receiveSenderId = payload.SenderId;
                _receiveStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                _receiveTotal = payload.FileSize;
                _receiveWritten = 0;
            }
            FileReceiveStarted?.Invoke(this, (payload.FileName, payload.SenderId));
        }

        public void ReceiveChunk(byte[] payload)
        {
            lock (_receiveLock)
            {
                if (_receiveStream is null)
                    return;
                if (_receiveWritten + payload.Length > _receiveTotal)
                {
                    _receiveStream.Dispose();
                    _receiveStream = null;
                    return;
                }
                _receiveStream.Write(payload, 0, payload.Length);
                _receiveWritten += payload.Length;
            }
        }

        public void EndReceive(FileEndPayload? _)
        {
            string? path;
            string? name;
            lock (_receiveLock)
            {
                _receiveStream?.Dispose();
                _receiveStream = null;
                path = _receiveFilePath;
                name = _receiveFileName;
                _receiveFilePath = null;
                _receiveFileName = null;
                _receiveTotal = 0;
                _receiveWritten = 0;
            }
            if (name is not null && path is not null)
                FileReceiveCompleted?.Invoke(this, (name, path));
        }
    }
}
