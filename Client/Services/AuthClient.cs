using System.Text.Json;
using Client.Protocol;
using Shared.DTO;
using Shared.Models;
using Shared.Protocol;

namespace Client.Services
{
    public sealed class AuthClient
    {
        private readonly PacketReader _reader;
        private readonly PacketWriter _writer;
        private readonly SessionContext _sessionContext;

        public AuthClient(PacketReader reader, PacketWriter writer, SessionContext sessionContext)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _sessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));
        }

        public async Task<RegisterResult> RegisterAsync(string login, string password, CancellationToken cancellationToken = default)
        {
            var request = new RegisterRequest { Login = login, Password = password };
            var payload = JsonSerializer.SerializeToUtf8Bytes(request, ClientProtocolConstants.JsonOptions);
            await _writer.WritePacketAsync(MessageType.Register, payload, cancellationToken).ConfigureAwait(false);

            var (type, responsePayload) = await _reader.ReadPacketAsync(cancellationToken).ConfigureAwait(false);
            if (type == MessageType.Error)
            {
                var err = JsonSerializer.Deserialize<ErrorPayload>(responsePayload, ClientProtocolConstants.JsonOptions);
                return RegisterResult.Fail(err?.Code ?? "Error", err?.Message ?? "Unknown error");
            }
            if (type == MessageType.Ack)
                return RegisterResult.Ok();
            return RegisterResult.Fail("UnexpectedResponse", $"Unexpected message type: {type}");
        }

        public async Task<LoginResult> LoginAsync(string login, string password, CancellationToken cancellationToken = default)
        {
            var request = new LoginRequest { Login = login, Password = password };
            var payload = JsonSerializer.SerializeToUtf8Bytes(request, ClientProtocolConstants.JsonOptions);
            await _writer.WritePacketAsync(MessageType.Login, payload, cancellationToken).ConfigureAwait(false);

            var (type, responsePayload) = await _reader.ReadPacketAsync(cancellationToken).ConfigureAwait(false);
            if (type == MessageType.Error)
            {
                var err = JsonSerializer.Deserialize<ErrorPayload>(responsePayload, ClientProtocolConstants.JsonOptions);
                return LoginResult.Fail(err?.Code ?? "Error", err?.Message ?? "Unknown error");
            }
            if (type == MessageType.Login)
            {
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responsePayload, ClientProtocolConstants.JsonOptions);
                if (loginResponse is not null && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    _sessionContext.Token = loginResponse.Token;
                    _sessionContext.UserId = loginResponse.UserId;
                    return LoginResult.Ok();
                }
                return LoginResult.Fail("InvalidResponse", "Login response missing token.");
            }
            return LoginResult.Fail("UnexpectedResponse", $"Unexpected message type: {type}");
        }
    }
}
