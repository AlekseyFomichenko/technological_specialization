namespace Client.Services
{
    public sealed class RegisterResult
    {
        public bool Success { get; init; }
        public string? ErrorCode { get; init; }
        public string? ErrorMessage { get; init; }

        public static RegisterResult Ok() => new() { Success = true };
        public static RegisterResult Fail(string code, string message) => new()
        {
            Success = false,
            ErrorCode = code,
            ErrorMessage = message
        };
    }
}
