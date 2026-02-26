namespace Client.Services
{
    public sealed class PendingResponseResult
    {
        public bool Success { get; init; }
        public string? ErrorCode { get; init; }
        public string? ErrorMessage { get; init; }

        public static PendingResponseResult Ok() => new() { Success = true };
        public static PendingResponseResult Fail(string code, string message) => new()
        {
            Success = false,
            ErrorCode = code,
            ErrorMessage = message
        };
    }
}
