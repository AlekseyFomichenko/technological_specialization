namespace Client.Services
{
    public sealed class SendFileResult
    {
        public bool Success { get; init; }
        public string? ErrorCode { get; init; }
        public string? ErrorMessage { get; init; }

        public static SendFileResult Ok() => new() { Success = true };
        public static SendFileResult Fail(string code, string message) => new()
        {
            Success = false,
            ErrorCode = code,
            ErrorMessage = message
        };
    }
}
