namespace Shared.DTO
{
    public class TextMessagePayload
    {
        public string Token { get; set; } = string.Empty;
        public string ReceiverLogin { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
