namespace Server.Models
{
    internal class Session
    {
        public Guid Id { get; set; }
        public string UserLogin { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
