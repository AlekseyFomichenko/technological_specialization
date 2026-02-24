using Shared.Models;

namespace Server.Services.Abstracts
{
    internal interface IMessageDelivery
    {
        Task<bool> SendToUserAsync(Guid userId, MessageType messageType, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default);
    }
}
