namespace API.DTOs
{
    public class RestoreResponse
    {
        public Guid Id { get; set; }
        public Guid BackupId { get; set; }
        public Guid RequestedBy { get; set; }
        public string RequesterName { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string? TargetDb { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public string? Notes { get; set; }
    }
}
