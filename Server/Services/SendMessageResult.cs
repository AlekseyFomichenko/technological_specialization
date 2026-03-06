namespace Server.Services
{
    internal sealed class SendMessageResult
    {
        public bool Success { get; }
        public Guid? MessageId { get; }
        public string? ErrorCode { get; }
        public string? ErrorMessage { get; }

        private SendMessageResult(bool success, Guid? messageId, string? errorCode, string? errorMessage)
        {
            Success = success;
            MessageId = messageId;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public static SendMessageResult Ok(Guid messageId) => new(true, messageId, null, null);
        public static SendMessageResult Fail(string code, string message) => new(false, null, code, message);
    }
}
