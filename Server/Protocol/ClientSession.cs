using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Server.Models;
using Server.Options;
using Server.Services;
using Server.Services.Abstracts;
using Shared.DTO;
using Shared.Models;
using Shared.Protocol;

namespace Server.Protocol
{
    internal sealed class ClientSession
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly Stream _stream;
        private readonly PacketReader _reader;
        private readonly PacketWriter _writer;
        private readonly IPAddress? _clientIp;
        private readonly IAuthService _authService;
        private readonly IMessageService _messageService;
        private readonly IFileTransferService _fileTransferService;
        private readonly ILogger<ClientSession> _logger;
        private readonly Action<Guid> _onTerminated;
        private readonly Action<Guid, string>? _onAuthenticated;
        private readonly int _inactivityTimeoutMinutes;
        private readonly SemaphoreSlim _writeLock = new(1, 1);

        private ClientSessionState _state;
        private string? _login;
        private string? _token;
        private int _invalidPacketCount;

        public Guid ConnectionId { get; }

        public ClientSession(
            Stream stream,
            IPAddress? clientIp,
            Action<Guid> onTerminated,
            Action<Guid, string>? onAuthenticated,
            IAuthService authService,
            IMessageService messageService,
            IFileTransferService fileTransferService,
            IOptions<ServerSessionOptions> sessionOptions,
            ILogger<ClientSession> logger)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _clientIp = clientIp;
            _onTerminated = onTerminated ?? throw new ArgumentNullException(nameof(onTerminated));
            _onAuthenticated = onAuthenticated;
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _fileTransferService = fileTransferService ?? throw new ArgumentNullException(nameof(fileTransferService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _inactivityTimeoutMinutes = sessionOptions?.Value?.InactivityTimeoutMinutes ?? 5;

            ConnectionId = Guid.NewGuid();
            _state = ClientSessionState.Connected;
            _reader = new PacketReader(stream);
            _writer = new PacketWriter(stream);
        }

        public async Task<bool> TrySendAsync(MessageType type, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default)
        {
            await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_state == ClientSessionState.Terminated)
                    return false;
                await _writer.WritePacketAsync(type, payload, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            _state = ClientSessionState.AwaitingAuth;
            _invalidPacketCount = 0;

            try
            {
                while (_state != ClientSessionState.Terminated)
                {
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    linkedCts.CancelAfter(TimeSpan.FromMinutes(_inactivityTimeoutMinutes));

                    (MessageType type, byte[] payload) = await _reader.ReadPacketAsync(linkedCts.Token).ConfigureAwait(false);
                    _invalidPacketCount = 0;

                    bool handled = await DispatchAsync(type, payload, cancellationToken).ConfigureAwait(false);
                    if (!handled)
                    {
                        _invalidPacketCount++;
                        if (_invalidPacketCount >= 2)
                        {
                            _state = ClientSessionState.Terminated;
                            break;
                        }
                        await SendErrorAsync(ErrorCodes.InvalidState, "Packet not allowed in current state.").ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _state = ClientSessionState.Terminated;
            }
            catch (ProtocolException ex)
            {
                _logger.LogDebug(ex, "Protocol error for connection {ConnectionId}", ConnectionId);
                _state = ClientSessionState.Terminated;
            }
            catch (IOException ex)
            {
                _logger.LogDebug(ex, "Connection closed for {ConnectionId}", ConnectionId);
                _state = ClientSessionState.Terminated;
            }
            finally
            {
                await TerminateAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<bool> DispatchAsync(MessageType type, byte[] payload, CancellationToken cancellationToken)
        {
            switch (_state)
            {
                case ClientSessionState.AwaitingAuth:
                    return type switch
                    {
                        MessageType.Register => await HandleRegisterAsync(payload, cancellationToken).ConfigureAwait(false),
                        MessageType.Login => await HandleLoginAsync(payload, cancellationToken).ConfigureAwait(false),
                        _ => false
                    };
                case ClientSessionState.Blocked:
                    await SendErrorAsync(ErrorCodes.Blocked, "Too many failed attempts.").ConfigureAwait(false);
                    return true;
                case ClientSessionState.Authenticated:
                    return type switch
                    {
                        MessageType.TextMessage => await HandleTextMessageAsync(payload, cancellationToken).ConfigureAwait(false),
                        MessageType.FileStart => await HandleFileStartAsync(payload, cancellationToken).ConfigureAwait(false),
                        MessageType.Register or MessageType.Login => false,
                        _ => false
                    };
                case ClientSessionState.ReceivingFile:
                    if (type == MessageType.FileChunk)
                        return await HandleFileChunkAsync(payload, cancellationToken).ConfigureAwait(false);
                    if (type == MessageType.FileEnd)
                        return await HandleFileEndAsync(cancellationToken).ConfigureAwait(false);
                    await _fileTransferService.CancelReceivingAsync(ConnectionId, cancellationToken).ConfigureAwait(false);
                    return false;
                default:
                    return false;
            }
        }

        private async Task<bool> HandleRegisterAsync(byte[] payload, CancellationToken cancellationToken)
        {
            RegisterRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<RegisterRequest>(payload, JsonOptions);
            }
            catch
            {
                await SendErrorAsync(ErrorCodes.InvalidInput, "Invalid payload.").ConfigureAwait(false);
                return true;
            }
            if (request is null)
            {
                await SendErrorAsync(ErrorCodes.InvalidInput, "Invalid payload.").ConfigureAwait(false);
                return true;
            }

            RegisterResult result = await _authService.RegisterAsync(request, cancellationToken).ConfigureAwait(false);
            if (result.Success)
            {
                await SendAckAsync(true, null).ConfigureAwait(false);
                return true;
            }
            await SendErrorAsync(result.ErrorCode ?? "ERROR", result.ErrorMessage ?? "Registration failed.").ConfigureAwait(false);
            return true;
        }

        private async Task<bool> HandleLoginAsync(byte[] payload, CancellationToken cancellationToken)
        {
            LoginRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<LoginRequest>(payload, JsonOptions);
            }
            catch
            {
                await SendErrorAsync(ErrorCodes.InvalidInput, "Invalid payload.").ConfigureAwait(false);
                return true;
            }
            if (request is null)
            {
                await SendErrorAsync(ErrorCodes.InvalidInput, "Invalid payload.").ConfigureAwait(false);
                return true;
            }

            LoginResult result = await _authService.LoginAsync(request, _clientIp, cancellationToken).ConfigureAwait(false);
            if (result.Success && result.Response is not null && result.Login is { } login)
            {
                _token = result.Response.Token;
                _login = login;
                _state = ClientSessionState.Authenticated;
                byte[] responseBytes = JsonSerializer.SerializeToUtf8Bytes(result.Response, JsonOptions);
                await _writer.WritePacketAsync(MessageType.Login, responseBytes, cancellationToken).ConfigureAwait(false);
                _onAuthenticated?.Invoke(ConnectionId, login);
                await _messageService.DeliverPendingForUserAsync(login, cancellationToken).ConfigureAwait(false);
                await _fileTransferService.DeliverPendingFilesForUserAsync(login, cancellationToken).ConfigureAwait(false);
                return true;
            }
            if (result.ErrorCode == ErrorCodes.Blocked)
            {
                await SendErrorAsync(ErrorCodes.Blocked, result.ErrorMessage ?? "Blocked.").ConfigureAwait(false);
                _state = ClientSessionState.Blocked;
                return true;
            }
            await SendErrorAsync(result.ErrorCode ?? ErrorCodes.InvalidCredentials, result.ErrorMessage ?? "Invalid credentials.").ConfigureAwait(false);
            return true;
        }

        private async Task<bool> HandleTextMessageAsync(byte[] payload, CancellationToken cancellationToken)
        {
            if (_login is not { } senderLogin)
                return false;
            TextMessagePayload? messagePayload;
            try
            {
                messagePayload = JsonSerializer.Deserialize<TextMessagePayload>(payload, JsonOptions);
            }
            catch
            {
                await SendErrorAsync(ErrorCodes.InvalidInput, "Invalid payload.").ConfigureAwait(false);
                return true;
            }
            if (messagePayload is null)
            {
                await SendErrorAsync(ErrorCodes.InvalidInput, "Invalid payload.").ConfigureAwait(false);
                return true;
            }

            SendMessageResult result = await _messageService.SendMessageAsync(senderLogin, messagePayload, cancellationToken).ConfigureAwait(false);
            if (result.Success)
                return true;
            await SendErrorAsync(result.ErrorCode ?? "ERROR", result.ErrorMessage ?? "Send failed.").ConfigureAwait(false);
            return true;
        }

        private async Task<bool> HandleFileStartAsync(byte[] payload, CancellationToken cancellationToken)
        {
            if (_login is not { } senderLogin)
                return false;
            FileStartPayload? filePayload;
            try
            {
                filePayload = JsonSerializer.Deserialize<FileStartPayload>(payload, JsonOptions);
            }
            catch
            {
                await SendErrorAsync(ErrorCodes.InvalidInput, "Invalid payload.").ConfigureAwait(false);
                return true;
            }
            if (filePayload is null)
            {
                await SendErrorAsync(ErrorCodes.InvalidInput, "Invalid payload.").ConfigureAwait(false);
                return true;
            }

            StartFileResult result = await _fileTransferService.StartReceivingAsync(ConnectionId, senderLogin, filePayload, cancellationToken).ConfigureAwait(false);
            if (result.Success)
            {
                _state = ClientSessionState.ReceivingFile;
                await _writer.WritePacketAsync(MessageType.FileStartAck, Array.Empty<byte>(), cancellationToken).ConfigureAwait(false);
                return true;
            }
            _logger.LogWarning("File start rejected: ConnectionId={ConnectionId}, Code={Code}, Message={Message}, Sender={Sender}, File={FileName}, Size={FileSize}",
                ConnectionId, result.ErrorCode, result.ErrorMessage, senderLogin, filePayload.FileName, filePayload.FileSize);
            await SendErrorAsync(result.ErrorCode ?? "ERROR", result.ErrorMessage ?? "File start failed.").ConfigureAwait(false);
            return true;
        }

        private async Task<bool> HandleFileChunkAsync(byte[] payload, CancellationToken cancellationToken)
        {
            WriteChunkResult result = await _fileTransferService.WriteChunkAsync(ConnectionId, payload.AsMemory(), cancellationToken).ConfigureAwait(false);
            if (result.Success)
                return true;
            _logger.LogWarning("File chunk rejected: ConnectionId={ConnectionId}, Code={Code}, Message={Message}",
                ConnectionId, result.ErrorCode, result.ErrorMessage);
            await _fileTransferService.CancelReceivingAsync(ConnectionId, cancellationToken).ConfigureAwait(false);
            await SendErrorAsync(result.ErrorCode ?? ErrorCodes.SizeExceeded, result.ErrorMessage ?? "Chunk write failed.").ConfigureAwait(false);
            _state = ClientSessionState.Terminated;
            return true;
        }

        private async Task<bool> HandleFileEndAsync(CancellationToken cancellationToken)
        {
            EndFileResult result = await _fileTransferService.EndReceivingAsync(ConnectionId, cancellationToken).ConfigureAwait(false);
            if (result.Success && result.FileId is { } fileId)
            {
                _state = ClientSessionState.Authenticated;
                await SendAckAsync(true, fileId).ConfigureAwait(false);
                return true;
            }
            _logger.LogWarning("File end rejected: ConnectionId={ConnectionId}, Code={Code}, Message={Message}",
                ConnectionId, result.ErrorCode, result.ErrorMessage);
            await SendErrorAsync(result.ErrorCode ?? "ERROR", result.ErrorMessage ?? "File end failed.").ConfigureAwait(false);
            _state = ClientSessionState.Authenticated;
            return true;
        }

        private async Task SendErrorAsync(string code, string message)
        {
            var error = new ErrorPayload { Code = code, Message = message };
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(error, JsonOptions);
            await _writer.WritePacketAsync(MessageType.Error, bytes, CancellationToken.None).ConfigureAwait(false);
        }

        private async Task SendAckAsync(bool success, Guid? id)
        {
            var ack = new AckPayload { Success = success, Id = id };
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(ack, JsonOptions);
            await _writer.WritePacketAsync(MessageType.Ack, bytes, CancellationToken.None).ConfigureAwait(false);
        }

        private async Task TerminateAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _fileTransferService.CancelReceivingAsync(ConnectionId, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "CancelReceivingAsync failed for {ConnectionId}", ConnectionId);
            }

            try
            {
                _stream.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Stream dispose failed for {ConnectionId}", ConnectionId);
            }

            _onTerminated(ConnectionId);
        }
    }
}
