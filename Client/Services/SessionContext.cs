namespace Client.Services
{
    public sealed class SessionContext
    {
        public string? Token { get; set; }
        public Guid? UserId { get; set; }
    }
}
