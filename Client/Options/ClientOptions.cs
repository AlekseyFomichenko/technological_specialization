namespace Client.Options
{
    public class ClientOptions
    {
        public string ServerAddress { get; set; } = "127.0.0.1";
        public int ServerPort { get; set; } = 5001;
        public int FileChunkSizeBytes { get; set; } = 65536;
        public string DownloadsPath { get; set; } = "./Downloads";
    }
}
