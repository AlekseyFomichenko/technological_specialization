namespace Shared.DTO
{
    public class FileStartPayload
    {
        public string Token { get; set; } = string.Empty;
        public string ReceiverLogin { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}
