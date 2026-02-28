using Shared.Models;

namespace Server.Services.Abstracts
{
    internal interface IMessageDelivery
    {
        Task<bool> SendToUserAsync(string login, MessageType messageType, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default);
    }
}
