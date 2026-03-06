using Shared.DTO;

namespace Server.Services
{
    internal sealed class LoginResult
    {
        public bool Success { get; }
        public LoginResponse? Response { get; }
        public string? Login { get; }
        public string? ErrorCode { get; }
        public string? ErrorMessage { get; }

        private LoginResult(bool success, LoginResponse? response, string? login, string? errorCode, string? errorMessage)
        {
            Success = success;
            Response = response;
            Login = login;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public static LoginResult Ok(LoginResponse response, string login) => new(true, response, login, null, null);
        public static LoginResult Fail(string code, string message) => new(false, null, null, code, message);
    }
}
