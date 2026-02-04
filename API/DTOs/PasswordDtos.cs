namespace API.DTOs
{
    public class PasswordRecoveryRequest
    {
        public string Email { get; set; } = default!;
    }

    public class PasswordResetRequest
    {
        public string Token { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }

    public class PasswordChangeRequest
    {
        public Guid UserId { get; set; }
        public string CurrentPassword { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
