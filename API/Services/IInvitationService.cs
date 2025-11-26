using API.DTOs;

namespace API.Services
{
    public interface IInvitationService
    {
        Task<InvitationResponse?> InviteUserAsync(InviteUserRequest request, Guid invitedByUserId, string ipAddress, string userAgent);
        Task<ValidateInvitationResponse> ValidateTokenAsync(string token);
        Task<AuthResponse?> CompleteInvitationAsync(CompleteInvitationRequest request, string ipAddress, string userAgent);
    }
}
