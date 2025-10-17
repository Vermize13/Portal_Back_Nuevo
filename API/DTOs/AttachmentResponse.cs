namespace API.DTOs
{
    public class AttachmentResponse
    {
        public Guid Id { get; set; }
        public Guid IncidentId { get; set; }
        public Guid UploadedBy { get; set; }
        public string UploaderName { get; set; } = default!;
        public string FileName { get; set; } = default!;
        public string MimeType { get; set; } = default!;
        public long FileSizeBytes { get; set; }
        public DateTimeOffset UploadedAt { get; set; }
    }
}
