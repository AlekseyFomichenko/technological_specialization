namespace ChatServer.Database.Entities
{
    public class FileEntity
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? MimeType { get; set; }
        public long Size { get; set; }
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public DateTime UploadedAt { get; set; }
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public virtual UserEntity? Sender { get; set; }
        public virtual UserEntity? Recipient { get; set; }
    }
}
