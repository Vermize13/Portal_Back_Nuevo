namespace API.DTOs
{
    public class RestoreRequest
    {
        public Guid BackupId { get; set; }
        public string? Notes { get; set; }
    }
}
