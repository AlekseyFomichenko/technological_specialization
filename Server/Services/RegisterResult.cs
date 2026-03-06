namespace Server.Services
{
    internal sealed class RegisterResult
    {
        public bool Success { get; }
        public string? ErrorCode { get; }
        public string? ErrorMessage { get; }

        private RegisterResult(bool success, string? errorCode, string? errorMessage)
        {
            Success = success;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public static RegisterResult Ok() => new(true, null, null);
        public static RegisterResult Fail(string code, string message) => new(false, code, message);
    }
}
