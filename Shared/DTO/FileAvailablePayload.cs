namespace Shared.DTO
{
    public class FileAvailablePayload
    {
        public Guid FileId { get; set; }
        public Guid SenderId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}
