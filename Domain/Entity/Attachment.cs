using System;

namespace Domain.Entity
{
    public class Attachment
    {
        public Guid Id { get; set; }
        public Guid IncidentId { get; set; }
        public Incident Incident { get; set; } = default!;
        public Guid UploadedBy { get; set; }
        public User Uploader { get; set; } = default!;
        public string FileName { get; set; } = default!;
        public string StoragePath { get; set; } = default!;
        public string MimeType { get; set; } = default!;
        public long FileSizeBytes { get; set; }
        public string? Sha256Checksum { get; set; }
        public DateTimeOffset UploadedAt { get; set; }
    }
}
