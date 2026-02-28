namespace Shared.DTO
{
    public class IncomingTextPayload
    {
        public string SenderLogin { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Guid MessageId { get; set; }
    }
}
