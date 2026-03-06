using Microsoft.EntityFrameworkCore;
using Server.Data.Abstracts;
using Server.Models;

namespace Server.Data
{
    internal class SessionRepository : ISessionRepository
    {
        private readonly ChatDbContext _context;

        public SessionRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Session session, CancellationToken cancellationToken = default)
        {
            await _context.Sessions.AddAsync(session, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Session?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _context.Sessions.FirstOrDefaultAsync(s => s.Token == token, cancellationToken);
        }

        public async Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
        {
            List<Session>? expired = await _context.Sessions
                .Where(s => s.ExpiresAt < DateTime.UtcNow)
                .ToListAsync(cancellationToken);
            _context.Sessions.RemoveRange(expired);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
