using Server.Data.Abstracts;
using Server.Services.Abstracts;

namespace Server.Services
{
    internal class SessionTokenValidator : ITokenValidator
    {
        private readonly ISessionRepository _sessionRepository;

        public SessionTokenValidator(ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        public async Task<Guid?> ValidateAsync(string token, CancellationToken cancellationToken = default)
        {
            var session = await _sessionRepository.GetByTokenAsync(token, cancellationToken);
            if (session is null || session.ExpiresAt < DateTime.UtcNow)
                return null;
            return session.UserId;
        }
    }
}
