namespace API.DTOs
{
    public class AuthResponse
    {
        public string Token { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? Role { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
