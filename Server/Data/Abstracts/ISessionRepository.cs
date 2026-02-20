using Server.Models;

namespace Server.Data.Abstracts
{
    internal interface ISessionRepository
    {
        Task AddAsync(Session session, CancellationToken cancellationToken = default);
        Task<Session?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        Task DeleteExpiredAsync(CancellationToken cancellationToken = default);
    }
}
