using Server.Models;

namespace Server.Data.Abstracts
{
    internal interface IMessageRepository
    {
        Task AddAsync(Message message, CancellationToken cancellationToken = default);
        Task UpdateDeliveredAsync(Guid messageId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Message>> GetUndeliveredForUserAsync(string receiverLogin, CancellationToken cancellationToken = default);
    }
}
