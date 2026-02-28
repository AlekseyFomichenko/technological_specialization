using Server.Models;

namespace Server.Data.Abstracts
{
    internal interface IUserRepository
    {
        Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default);
        Task AddAsync(User user, CancellationToken cancellationToken = default);
    }
}
