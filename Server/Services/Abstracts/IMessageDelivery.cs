using Shared.Models;

namespace Server.Services.Abstracts
{
    internal interface IMessageDelivery
    {
        Task SendToUserAsync(Guid userId, MessageType messageType, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default);
    }
}
