
namespace API.Services
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string email, string token);
    }
}
