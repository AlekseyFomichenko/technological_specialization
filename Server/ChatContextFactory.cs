using ChatServer.Database;
using Microsoft.EntityFrameworkCore;

namespace ServerHost
{
    public class ChatContextFactory : IDbContextFactory<ChatContext>
    {
        private readonly DbContextOptions<ChatContext> _options;

        public ChatContextFactory(DbContextOptions<ChatContext> options)
        {
            _options = options;
        }

        public ChatContext CreateDbContext() => new ChatContext(_options);

        public Task<ChatContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new ChatContext(_options));
    }
}
