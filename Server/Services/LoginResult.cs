using Shared.DTO;

namespace Server.Services
{
    internal sealed class LoginResult
    {
        public bool Success { get; }
        public LoginResponse? Response { get; }
        public Guid? UserId { get; }
        public string? ErrorCode { get; }
        public string? ErrorMessage { get; }

        private LoginResult(bool success, LoginResponse? response, Guid? userId, string? errorCode, string? errorMessage)
        {
            Success = success;
            Response = response;
            UserId = userId;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public static LoginResult Ok(LoginResponse response, Guid userId) => new(true, response, userId, null, null);
        public static LoginResult Fail(string code, string message) => new(false, null, null, code, message);
    }
}
