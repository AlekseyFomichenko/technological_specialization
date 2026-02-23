using Server.Data.Abstracts;
using Server.Models;
using Server.Services.Abstracts;
using Shared.DTO;

namespace Server.Services
{
    internal class AuthService
    {
        private const int MinLoginLength = 3;
        private const int MinPasswordLength = 6;

        private static class ErrorCodes
        {
            internal const string LoginRequired = "LOGIN_REQUIRED";
            internal const string PasswordRequired = "PASSWORD_REQUIRED";
            internal const string LoginTooShort = "LOGIN_TOO_SHORT";
            internal const string PasswordTooWeak = "PASSWORD_TOO_WEAK";
            internal const string LoginTaken = "LOGIN_TAKEN";
        }

        private readonly IUserRepository _userRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly ITokenValidator _tokenValidator;
        private readonly ILoginAttemptTracker _loginAttemptTracker;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            ISessionRepository sessionRepository,
            IPasswordHasher passwordHasher,
            ITokenGenerator tokenGenerator,
            ITokenValidator tokenValidator,
            ILoginAttemptTracker loginAttemptTracker,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
            _tokenValidator = tokenValidator;
            _loginAttemptTracker = loginAttemptTracker;
            _logger = logger;
        }

        public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Login))
                return RegisterResult.Fail(ErrorCodes.LoginRequired, "Login is required.");

            if (string.IsNullOrWhiteSpace(request.Password))
                return RegisterResult.Fail(ErrorCodes.PasswordRequired, "Password is required.");

            if (request.Login.Length < MinLoginLength)
                return RegisterResult.Fail(ErrorCodes.LoginTooShort, $"Login must be at least {MinLoginLength} characters.");

            if (request.Password.Length < MinPasswordLength)
                return RegisterResult.Fail(ErrorCodes.PasswordTooWeak, $"Password must be at least {MinPasswordLength} characters.");

            User? existing = await _userRepository.GetByLoginAsync(request.Login, cancellationToken).ConfigureAwait(false);
            if (existing is not null)
                return RegisterResult.Fail(ErrorCodes.LoginTaken, "Login is already taken.");

            string hash = _passwordHasher.Hash(request.Password);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Login = request.Login.Trim(),
                PasswordHash = hash,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, cancellationToken).ConfigureAwait(false);
            return RegisterResult.Ok();
        }
    }
}
