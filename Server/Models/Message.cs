namespace Server.Models
{
    internal class Message
    {
        public Guid Id { get; set; }
        public string SenderLogin { get; set; } = string.Empty;
        public string ReceiverLogin { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsDelivered { get; set; }
    }
}
