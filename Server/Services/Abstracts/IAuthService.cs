using System.Net;
using Shared.DTO;

namespace Server.Services.Abstracts
{
    internal interface IAuthService
    {
        Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
        Task<LoginResult> LoginAsync(LoginRequest request, IPAddress? clientIp, CancellationToken cancellationToken = default);
    }
}
