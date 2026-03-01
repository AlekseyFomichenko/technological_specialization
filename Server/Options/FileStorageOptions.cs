namespace Server.Options
{
    internal class FileStorageOptions
    {
        public string BasePath { get; set; } = string.Empty;
        public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
        public int MaxFileSizeMb { get; set; } = 50;
    }
}
