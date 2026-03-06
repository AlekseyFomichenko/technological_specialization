namespace Server.Services
{
    internal sealed class EndFileResult
    {
        public bool Success { get; }
        public Guid? FileId { get; }
        public string? ErrorCode { get; }
        public string? ErrorMessage { get; }

        private EndFileResult(bool success, Guid? fileId, string? errorCode, string? errorMessage)
        {
            Success = success;
            FileId = fileId;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public static EndFileResult Ok(Guid fileId) => new(true, fileId, null, null);
        public static EndFileResult Fail(string code, string message) => new(false, null, code, message);
    }
}
