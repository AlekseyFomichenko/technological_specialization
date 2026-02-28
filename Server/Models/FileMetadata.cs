namespace Server.Models
{
    internal class FileMetadata
    {
        public Guid Id { get; set; }
        public string SenderLogin { get; set; } = string.Empty;
        public string ReceiverLogin { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDelivered { get; set; }
    }
}
