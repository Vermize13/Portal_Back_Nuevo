namespace API.DTOs
{
    public class BackupResponse
    {
        public Guid Id { get; set; }
        public Guid CreatedBy { get; set; }
        public string CreatorName { get; set; } = default!;
        public string StoragePath { get; set; } = default!;
        public string Strategy { get; set; } = default!;
        public long? SizeBytes { get; set; }
        public string Status { get; set; } = default!;
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public string? Notes { get; set; }
    }
}
