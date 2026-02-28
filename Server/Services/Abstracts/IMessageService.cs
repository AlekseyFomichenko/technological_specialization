using Shared.DTO;

namespace Server.Services.Abstracts
{
    internal interface IMessageService
    {
        Task<SendMessageResult> SendMessageAsync(Guid senderId, TextMessagePayload payload, CancellationToken cancellationToken = default);
        Task DeliverPendingForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
