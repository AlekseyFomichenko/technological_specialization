using Microsoft.EntityFrameworkCore;
using Server.Data.Abstracts;
using Server.Models;

namespace Server.Data
{
    internal class MessageRepository : IMessageRepository
    {
        private readonly ChatDbContext _context;

        public MessageRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Message message, CancellationToken cancellationToken = default)
        {
            await _context.Messages.AddAsync(message, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateDeliveredAsync(Guid messageId, CancellationToken cancellationToken = default)
        {
            Message? message = await _context.Messages.FindAsync([messageId], cancellationToken);
            if (message is not null)
            {
                message.IsDelivered = true;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IReadOnlyList<Message>> GetUndeliveredForUserAsync(string receiverLogin, CancellationToken cancellationToken = default)
        {
            return await _context.Messages
                .Where(m => m.ReceiverLogin == receiverLogin && !m.IsDelivered)
                .ToListAsync(cancellationToken);
        }
    }
}
