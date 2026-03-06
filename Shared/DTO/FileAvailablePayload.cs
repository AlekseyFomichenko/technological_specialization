namespace Shared.DTO
{
    public class FileAvailablePayload
    {
        public Guid FileId { get; set; }
        public string SenderLogin { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}
