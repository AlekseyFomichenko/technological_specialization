namespace Server.Services
{
    internal sealed class WriteChunkResult
    {
        public bool Success { get; }
        public string? ErrorCode { get; }
        public string? ErrorMessage { get; }

        private WriteChunkResult(bool success, string? errorCode, string? errorMessage)
        {
            Success = success;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public static WriteChunkResult Ok() => new(true, null, null);
        public static WriteChunkResult Fail(string code, string message) => new(false, code, message);
    }
}
