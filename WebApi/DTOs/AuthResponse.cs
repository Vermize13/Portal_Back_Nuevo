namespace WebApi.DTOs
{
    public class AuthResponse
    {
        public string Token { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public List<string> Roles { get; set; } = new();
        public DateTime ExpiresAt { get; set; }
    }
}
