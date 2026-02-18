using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ChatContracts;
using ChatServer.Database;
using ChatServer.Database.Entities;
using ChatTransport.Abstracts;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ChatServer.Core
{
    public class Server
    {
        private const int HistoryLimit = 50;
        private const long MaxFileSizeBytes = 500L * 1024 * 1024;

        private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/gif", "audio/mpeg", "audio/wav", "video/mp4"
        };

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".mp3", ".wav", ".mp4"
        };

        private readonly Dictionary<string, IPEndPoint> _clients = new();
        private readonly IMessageSourceServer _messageSource;
        private readonly IDbContextFactory<ChatContext> _contextFactory;
        private readonly ConcurrentQueue<PendingFileUpload> _pendingUploads = new();
        private IPEndPoint _ep;
        private bool _isWork = true;

        private sealed record PendingFileUpload(string From, string To, string FileName, string? MimeType, long Size);

        public Server(IMessageSourceServer messageSource, IDbContextFactory<ChatContext> contextFactory)
        {
            _messageSource = messageSource;
            _contextFactory = contextFactory;
            _ep = _messageSource.CreateEndPoint();
        }

        public void Stop() => _isWork = false;

        private static (string? Normalized, ErrorCode? ErrorCode, string? ErrorDescription) TryNormalizeNick(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (null, ErrorCode.InvalidNick, "Nick required");
            var trimmed = input.Trim();
            if (trimmed.Contains(' '))
                return (null, ErrorCode.InvalidNick, "Nick must not contain spaces");
            if (trimmed.Length < NickValidation.MinLength || trimmed.Length > NickValidation.MaxLength)
                return (null, ErrorCode.InvalidNick, "Nick length must be 4-12");
            return (trimmed.ToLowerInvariant(), null, null);
        }

        public async Task Run()
        {
            Console.WriteLine("Сервер ожидает сообщения");
            while (_isWork)
            {
                try
                {
                    var message = _messageSource.Receive(ref _ep);
                    await ProcessMessage(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private async Task ProcessMessage(NetMessage message)
        {
            switch (message.Command)
            {
                case Command.Register:
                    await Register(message);
                    break;
                case Command.Login:
                    await Login(message);
                    break;
                case Command.Message:
                    await RelayMessage(message);
                    break;
                case Command.Confirmation:
                    await ConfirmMessageReceived(message.Id);
                    break;
                case Command.FileOffer:
                    await HandleFileOffer(message);
                    break;
                default:
                    break;
            }
        }

        private async Task Register(NetMessage message)
        {
            if (message.EndPoint == null)
                return;
            var (normalized, nickError, nickDesc) = TryNormalizeNick(message.NickNameFrom);
            if (nickError != null)
            {
                await SendErrorAsync(message.EndPoint, nickError.Value, nickDesc ?? "");
                return;
            }
            if (string.IsNullOrEmpty(message.Password))
            {
                await SendErrorAsync(message.EndPoint, ErrorCode.Unauthorized, "Password required");
                return;
            }

            await using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var existing = await context.Users.FirstOrDefaultAsync(u => u.FullName == normalized);
                if (existing != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(message.Password, existing.PasswordHash))
                    {
                        _clients[normalized!] = _messageSource.CopyEndPoint(message.EndPoint);
                        await SendHistoryAsync(normalized!, _messageSource.CopyEndPoint(message.EndPoint));
                        Console.WriteLine($"Re-register name = {normalized}");
                    }
                    else
                        await SendErrorAsync(message.EndPoint, ErrorCode.Unauthorized, "Invalid password");
                    return;
                }

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(message.Password);
                context.Users.Add(new UserEntity
                {
                    FullName = normalized,
                    PasswordHash = passwordHash
                });
                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException ex) when (ex.InnerException is PostgresException pe && pe.SqlState == "23505")
                {
                    await SendErrorAsync(message.EndPoint, ErrorCode.NickTaken, "Nick already taken");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    await SendErrorAsync(message.EndPoint, ErrorCode.DbError, ex.Message);
                    return;
                }
            }

            _clients[normalized!] = _messageSource.CopyEndPoint(message.EndPoint);
            await SendHistoryAsync(normalized!, _messageSource.CopyEndPoint(message.EndPoint));
            Console.WriteLine($"Register name = {normalized}");
        }

        private async Task Login(NetMessage message)
        {
            if (message.EndPoint == null)
                return;
            var (normalized, nickError, nickDesc) = TryNormalizeNick(message.NickNameFrom);
            if (nickError != null)
            {
                await SendErrorAsync(message.EndPoint, nickError.Value, nickDesc ?? "");
                return;
            }
            if (string.IsNullOrEmpty(message.Password))
            {
                await SendErrorAsync(message.EndPoint, ErrorCode.Unauthorized, "Password required");
                return;
            }

            await using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.FullName == normalized);
                if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                {
                    await SendErrorAsync(message.EndPoint, ErrorCode.UserNotFound, "User not found");
                    return;
                }
                if (!BCrypt.Net.BCrypt.Verify(message.Password, user.PasswordHash))
                {
                    await SendErrorAsync(message.EndPoint, ErrorCode.Unauthorized, "Invalid password");
                    return;
                }
            }

            _clients[normalized!] = _messageSource.CopyEndPoint(message.EndPoint);
            await SendHistoryAsync(normalized!, _messageSource.CopyEndPoint(message.EndPoint));
            Console.WriteLine($"Login name = {normalized}");
        }

        private async Task SendErrorAsync(IPEndPoint to, ErrorCode code, string description)
        {
            var err = new NetMessage
            {
                Command = Command.Error,
                ErrorCode = code,
                ErrorDescription = description,
                Date = DateTime.UtcNow
            };
            await _messageSource.SendAsync(err, to);
        }

        private async Task SendHistoryAsync(string nick, IPEndPoint to)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var messages = await context.Messages
                .Include(m => m.UserFrom)
                .Include(m => m.UserTo)
                .Where(m => m.UserFrom!.FullName == nick || m.UserTo!.FullName == nick)
                .OrderByDescending(m => m.DateSend)
                .Take(HistoryLimit)
                .OrderBy(m => m.DateSend)
                .ToListAsync();

            foreach (var m in messages)
            {
                var fromName = m.UserFrom?.FullName ?? "";
                var toName = m.UserTo?.FullName ?? "";
                var msg = new NetMessage
                {
                    Command = Command.Message,
                    Id = m.MessageId,
                    Text = m.Text,
                    NickNameFrom = fromName,
                    NickNameTo = toName,
                    Date = m.DateSend
                };
                await _messageSource.SendAsync(msg, to);
            }
        }

        private async Task RelayMessage(NetMessage netMessage)
        {
            if (netMessage.EndPoint == null)
                return;
            var (fromNorm, fromErr, fromDesc) = TryNormalizeNick(netMessage.NickNameFrom);
            if (fromErr != null)
            {
                await SendErrorAsync(_messageSource.CopyEndPoint(netMessage.EndPoint), fromErr.Value, fromDesc ?? "");
                return;
            }
            var (toNorm, toErr, toDesc) = TryNormalizeNick(netMessage.NickNameTo);
            if (toErr != null)
            {
                await SendErrorAsync(_messageSource.CopyEndPoint(netMessage.EndPoint), ErrorCode.InvalidNick, toDesc ?? "Recipient nick invalid");
                return;
            }

            if (!_clients.TryGetValue(toNorm!, out IPEndPoint? recipientEp))
            {
                await SendErrorAsync(_messageSource.CopyEndPoint(netMessage.EndPoint), ErrorCode.RecipientNotFound, "Recipient not found");
                Console.WriteLine("Recipient not found: " + toNorm);
                return;
            }

            await using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    var fromUser = await context.Users.FirstOrDefaultAsync(x => x.FullName == fromNorm);
                    var toUser = await context.Users.FirstOrDefaultAsync(x => x.FullName == toNorm);
                    if (fromUser == null || toUser == null)
                    {
                        await SendErrorAsync(_messageSource.CopyEndPoint(netMessage.EndPoint), ErrorCode.RecipientNotFound, "User not found");
                        return;
                    }
                    var msg = new MessageEntity
                    {
                        UserFrom = fromUser,
                        UserTo = toUser,
                        IsSent = false,
                        Text = netMessage.Text ?? "",
                        DateSend = DateTime.UtcNow
                    };
                    context.Messages.Add(msg);
                    await context.SaveChangesAsync();
                    netMessage.Id = msg.MessageId;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    await SendErrorAsync(_messageSource.CopyEndPoint(netMessage.EndPoint), ErrorCode.DbError, ex.Message);
                    return;
                }
            }

            await _messageSource.SendAsync(netMessage, recipientEp);
            Console.WriteLine($"Message relayed from {fromNorm} to {toNorm}");
        }

        private async Task ConfirmMessageReceived(int? id)
        {
            if (id == null) return;
            Console.WriteLine("Message confirmation id = " + id);
            await using var context = await _contextFactory.CreateDbContextAsync();
            var msg = await context.Messages.FirstOrDefaultAsync(x => x.MessageId == id);
            if (msg != null)
            {
                msg.IsSent = true;
                await context.SaveChangesAsync();
            }
        }

        private async Task HandleFileOffer(NetMessage message)
        {
            if (message.EndPoint == null)
                return;
            var (fromNorm, fromErr, fromDesc) = TryNormalizeNick(message.NickNameFrom);
            if (fromErr != null)
            {
                await SendErrorAsync(_messageSource.CopyEndPoint(message.EndPoint), fromErr.Value, fromDesc ?? "");
                return;
            }
            var (toNorm, toErr, toDesc) = TryNormalizeNick(message.NickNameTo);
            if (toErr != null)
            {
                await SendErrorAsync(_messageSource.CopyEndPoint(message.EndPoint), ErrorCode.InvalidNick, toDesc ?? "Recipient nick invalid");
                return;
            }

            var senderEp = _messageSource.CopyEndPoint(message.EndPoint);

            if (!_clients.ContainsKey(toNorm!))
            {
                await SendErrorAsync(senderEp, ErrorCode.RecipientNotFound, "Recipient not found");
                return;
            }

            var size = message.FileSize ?? 0;
            if (size <= 0 || size > MaxFileSizeBytes)
            {
                await SendErrorAsync(senderEp, ErrorCode.FileTooLarge, "File size must be 1..500 MB");
                return;
            }

            var fileName = message.FileName ?? "";
            var ext = System.IO.Path.GetExtension(fileName);
            var mime = message.MimeType ?? "";
            if (string.IsNullOrEmpty(ext) && string.IsNullOrEmpty(mime))
            {
                await SendErrorAsync(senderEp, ErrorCode.InvalidFileFormat, "File name or MIME type required");
                return;
            }
            if (!AllowedExtensions.Contains(ext) && !AllowedMimeTypes.Contains(mime))
            {
                await SendErrorAsync(senderEp, ErrorCode.InvalidFileFormat, "File type not allowed");
                return;
            }

            var ack = new NetMessage
            {
                Command = Command.Ack,
                NickNameFrom = fromNorm,
                NickNameTo = toNorm,
                FileName = message.FileName,
                FileSize = message.FileSize,
                MimeType = message.MimeType,
                Date = DateTime.UtcNow
            };
            await _messageSource.SendAsync(ack, senderEp);
            _pendingUploads.Enqueue(new PendingFileUpload(
                fromNorm!,
                toNorm!,
                fileName,
                message.MimeType,
                size));
        }

        public async Task ProcessFileUploadAsync(Stream stream, long length, CancellationToken cancellationToken = default)
        {
            if (!_pendingUploads.TryDequeue(out var pending))
                return;
            if (length <= 0 || length > MaxFileSizeBytes)
                return;
            var content = new byte[(int)length];
            int offset = 0;
            while (offset < length)
            {
                int read = await stream.ReadAsync(content.AsMemory(offset, (int)Math.Min(length - offset, int.MaxValue)), cancellationToken);
                if (read == 0) return;
                offset += read;
            }
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var fromUser = await context.Users.FirstOrDefaultAsync(u => u.FullName == pending.From, cancellationToken);
            var toUser = await context.Users.FirstOrDefaultAsync(u => u.FullName == pending.To, cancellationToken);
            if (fromUser == null || toUser == null)
                return;
            var fileEntity = new FileEntity
            {
                FileName = pending.FileName,
                MimeType = pending.MimeType,
                Size = content.LongLength,
                Content = content,
                UploadedAt = DateTime.UtcNow,
                SenderId = fromUser.Id,
                RecipientId = toUser.Id
            };
            context.Files.Add(fileEntity);
            await context.SaveChangesAsync(cancellationToken);
            if (_clients.TryGetValue(pending.To, out var recipientEp))
            {
                var fileAvailable = new NetMessage
                {
                    Command = Command.FileAvailable,
                    NickNameFrom = pending.From,
                    NickNameTo = pending.To,
                    FileName = pending.FileName,
                    FileId = fileEntity.Id,
                    Date = DateTime.UtcNow
                };
                await _messageSource.SendAsync(fileAvailable, recipientEp);
            }
        }

        public async Task ProcessFileDownloadRequestAsync(Stream stream, int fileId, string? requesterNick, CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var file = await context.Files
                .Include(f => f.Recipient)
                .FirstOrDefaultAsync(f => f.Id == fileId, cancellationToken);
            if (file == null)
            {
                await WriteDownloadErrorAndCloseAsync(stream);
                return;
            }
            var recipientName = file.Recipient?.FullName;
            if (string.IsNullOrEmpty(requesterNick) || !string.Equals(recipientName, requesterNick.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                await WriteDownloadErrorAndCloseAsync(stream);
                return;
            }
            var length = file.Content.LongLength;
            var lengthBytes = BitConverter.GetBytes(length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);
            await stream.WriteAsync(lengthBytes, cancellationToken);
            var nameBytes = Encoding.UTF8.GetBytes(file.FileName ?? "");
            var nameLen = (ushort)Math.Min(nameBytes.Length, 65535);
            var nameLenBytes = BitConverter.GetBytes(nameLen);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(nameLenBytes);
            await stream.WriteAsync(nameLenBytes, cancellationToken);
            if (nameLen > 0)
                await stream.WriteAsync(nameBytes.AsMemory(0, nameLen), cancellationToken);
            await stream.WriteAsync(file.Content, cancellationToken);
        }

        private static async Task WriteDownloadErrorAndCloseAsync(Stream stream)
        {
            var lengthBytes = BitConverter.GetBytes(0L);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);
            await stream.WriteAsync(lengthBytes);
        }
    }
}
