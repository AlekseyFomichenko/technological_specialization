namespace Client.Services
{
    public sealed class SendMessageResult
    {
        public bool Success { get; init; }
        public string? ErrorCode { get; init; }
        public string? ErrorMessage { get; init; }

        public static SendMessageResult Ok() => new() { Success = true };
        public static SendMessageResult Fail(string code, string message) => new()
        {
            Success = false,
            ErrorCode = code,
            ErrorMessage = message
        };
    }
}
