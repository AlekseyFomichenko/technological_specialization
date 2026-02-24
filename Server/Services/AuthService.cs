using System.Net;
using Server.Data.Abstracts;
using Server.Models;
using Server.Services.Abstracts;
using Shared.DTO;

namespace Server.Services
{
    internal class AuthService : IAuthService
    {
        private const int MinLoginLength = 3;
        private const int MinPasswordLength = 6;
        private const int SessionLifetimeHours = 24;

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

        public async Task<LoginResult> LoginAsync(LoginRequest request, IPAddress? clientIp, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Login))
                return LoginResult.Fail(ErrorCodes.LoginRequired, "Login is required.");

            if (string.IsNullOrWhiteSpace(request.Password))
                return LoginResult.Fail(ErrorCodes.PasswordRequired, "Password is required.");

            if (_loginAttemptTracker.IsBlocked(clientIp, request.Login))
                return LoginResult.Fail(ErrorCodes.Blocked, "Too many failed attempts. Try again later.");

            User? user = await _userRepository.GetByLoginAsync(request.Login, cancellationToken).ConfigureAwait(false);
            if (user is null)
            {
                _loginAttemptTracker.RecordFailedAttempt(clientIp, request.Login);
                _logger.LogWarning("Failed login attempt for login '{Login}' from {Ip}", request.Login, clientIp?.ToString() ?? "unknown");
                return LoginResult.Fail(ErrorCodes.InvalidCredentials, "Invalid login or password.");
            }

            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                _loginAttemptTracker.RecordFailedAttempt(clientIp, request.Login);
                _logger.LogWarning("Failed login attempt for login '{Login}' from {Ip}", request.Login, clientIp?.ToString() ?? "unknown");
                return LoginResult.Fail(ErrorCodes.InvalidCredentials, "Invalid login or password.");
            }

            _loginAttemptTracker.ResetOnSuccess(clientIp, request.Login);
            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = _tokenGenerator.Generate(),
                ExpiresAt = DateTime.UtcNow.AddHours(SessionLifetimeHours)
            };
            await _sessionRepository.AddAsync(session, cancellationToken).ConfigureAwait(false);
            return LoginResult.Ok(new LoginResponse { Token = session.Token }, user.Id);
        }
    }
}
