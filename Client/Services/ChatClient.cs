using System.Text.Json;
using Client.Protocol;
using Shared.DTO;
using Shared.Models;
using Shared.Protocol;

namespace Client.Services
{
    public sealed class ChatClient
    {
        private readonly PacketWriter _writer;
        private readonly SessionContext _sessionContext;
        private readonly PendingResponse _pending;

        public ChatClient(PacketWriter writer, SessionContext sessionContext, PendingResponse pending)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _sessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));
            _pending = pending ?? throw new ArgumentNullException(nameof(pending));
        }

        public event EventHandler<IncomingTextPayload>? TextMessageReceived;

        internal void RaiseTextMessageReceived(IncomingTextPayload payload)
        {
            TextMessageReceived?.Invoke(this, payload);
        }

        public async Task<SendMessageResult> SendMessageAsync(Guid receiverId, string content, CancellationToken cancellationToken = default)
        {
            var token = _sessionContext.Token;
            if (string.IsNullOrEmpty(token))
                return SendMessageResult.Fail("NotAuthenticated", "Not logged in.");

            var payloadDto = new TextMessagePayload { Token = token, ReceiverId = receiverId, Content = content };
            var payload = JsonSerializer.SerializeToUtf8Bytes(payloadDto, ClientProtocolConstants.JsonOptions);
            _pending.SetPending();
            await _writer.WritePacketAsync(MessageType.TextMessage, payload, cancellationToken).ConfigureAwait(false);

            var result = await _pending.WaitAsync(cancellationToken).ConfigureAwait(false);
            return result.Success ? SendMessageResult.Ok() : SendMessageResult.Fail(result.ErrorCode ?? "Error", result.ErrorMessage ?? "Unknown");
        }
    }
}
