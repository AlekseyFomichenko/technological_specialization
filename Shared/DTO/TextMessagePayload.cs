namespace Shared.DTO
{
    public class TextMessagePayload
    {
        public string Token { get; set; } = string.Empty;
        public Guid ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
