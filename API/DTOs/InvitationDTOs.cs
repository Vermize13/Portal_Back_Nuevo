using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class InviteUserRequest
    {
        [Required]
        public string FullName { get; set; } = default!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public Guid RoleId { get; set; }
    }

    public class InvitationResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string RoleName { get; set; } = default!;
        public DateTimeOffset ExpiresAt { get; set; }
        public string Message { get; set; } = default!;
    }

    public class ValidateInvitationResponse
    {
        public bool IsValid { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? RoleName { get; set; }
        public string? Message { get; set; }
    }

    public class CompleteInvitationRequest
    {
        [Required]
        public string Token { get; set; } = default!;

        [Required]
        public string Username { get; set; } = default!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = default!;
    }
}
