namespace Server.Services
{
    internal sealed class StartFileResult
    {
        public bool Success { get; }
        public string? ErrorCode { get; }
        public string? ErrorMessage { get; }

        private StartFileResult(bool success, string? errorCode, string? errorMessage)
        {
            Success = success;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public static StartFileResult Ok() => new(true, null, null);
        public static StartFileResult Fail(string code, string message) => new(false, code, message);
    }
}
