namespace API.DTOs
{
    public class InvitationSettings
    {
        public string FrontendBaseUrl { get; set; } = default!;
        public int ExpirationHours { get; set; } = 48;
    }
}
