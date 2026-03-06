using Shared.DTO;

namespace Server.Services.Abstracts
{
    internal interface IMessageService
    {
        Task<SendMessageResult> SendMessageAsync(string senderLogin, TextMessagePayload payload, CancellationToken cancellationToken = default);
        Task DeliverPendingForUserAsync(string login, CancellationToken cancellationToken = default);
    }
}
