using System.Text.Json;
using Server.Data.Abstracts;
using Server.Models;
using Server.Services.Abstracts;
using Shared.DTO;
using Shared.Models;

namespace Server.Services
{
    internal class MessageService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMessageDelivery _messageDelivery;
        private readonly ILogger<MessageService> _logger;

        public MessageService(
            IMessageRepository messageRepository,
            IUserRepository userRepository,
            IMessageDelivery messageDelivery,
            ILogger<MessageService> logger)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _messageDelivery = messageDelivery;
            _logger = logger;
        }

        public async Task<SendMessageResult> SendMessageAsync(Guid senderId, TextMessagePayload payload, CancellationToken cancellationToken = default)
        {
            User? receiver = await _userRepository.GetByIdAsync(payload.ReceiverId, cancellationToken).ConfigureAwait(false);
            if (receiver is null)
                return SendMessageResult.Fail(ErrorCodes.ReceiverNotFound, "Receiver not found.");

            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = payload.ReceiverId,
                Content = payload.Content,
                CreatedAt = DateTime.UtcNow,
                IsDelivered = false
            };
            await _messageRepository.AddAsync(message, cancellationToken).ConfigureAwait(false);

            var incomingPayload = new IncomingTextPayload
            {
                SenderId = message.SenderId,
                Content = message.Content,
                MessageId = message.Id
            };
            byte[] incomingBytes = JsonSerializer.SerializeToUtf8Bytes(incomingPayload, JsonOptions);
            bool delivered = await _messageDelivery.SendToUserAsync(
                payload.ReceiverId,
                MessageType.TextMessage,
                incomingBytes.AsMemory(),
                cancellationToken).ConfigureAwait(false);
            if (delivered)
                await _messageRepository.UpdateDeliveredAsync(message.Id, cancellationToken).ConfigureAwait(false);

            var ackPayload = new AckPayload { Success = true, Id = message.Id };
            byte[] ackBytes = JsonSerializer.SerializeToUtf8Bytes(ackPayload, JsonOptions);
            await _messageDelivery.SendToUserAsync(
                senderId,
                MessageType.Ack,
                ackBytes.AsMemory(),
                cancellationToken).ConfigureAwait(false);

            return SendMessageResult.Ok(message.Id);
        }
    }
}
