namespace API.DTOs
{
    public class EmailSettings
    {
        public string ResendApiKey { get; set; } = default!;
        public string FromEmail { get; set; } = default!;
        public string FromName { get; set; } = default!;
    }
}
