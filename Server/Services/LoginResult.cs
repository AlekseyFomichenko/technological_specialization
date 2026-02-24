using Shared.DTO;

namespace Server.Services
{
    internal sealed class LoginResult
    {
        public bool Success { get; }
        public LoginResponse? Response { get; }
        public string? ErrorCode { get; }
        public string? ErrorMessage { get; }

        private LoginResult(bool success, LoginResponse? response, string? errorCode, string? errorMessage)
        {
            Success = success;
            Response = response;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public static LoginResult Ok(LoginResponse response) => new(true, response, null, null);
        public static LoginResult Fail(string code, string message) => new(false, null, code, message);
    }
}
