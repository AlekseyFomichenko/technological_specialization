namespace Shared.DTO
{
    public class IncomingTextPayload
    {
        public Guid SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid MessageId { get; set; }
    }
}
