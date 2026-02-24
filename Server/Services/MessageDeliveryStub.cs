using Shared.Models;
using Server.Services.Abstracts;

namespace Server.Services
{
    internal class MessageDeliveryStub : IMessageDelivery
    {
        public Task<bool> SendToUserAsync(Guid userId, MessageType messageType, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }
    }
}
