namespace Shared.DTO
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }
}
