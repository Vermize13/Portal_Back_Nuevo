using WebApi.DTOs;

namespace WebApi.Services
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginRequest request, string ipAddress, string userAgent);
        Task<AuthResponse?> RegisterAsync(RegisterRequest request, string ipAddress, string userAgent);
        string GenerateJwtToken(Guid userId, string username, List<string> roles);
    }
}
