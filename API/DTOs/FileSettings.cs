namespace API.DTOs
{
    public class FileSettings
    {
        public string StoragePath { get; set; } = default!;
        public long MaxFileSizeBytes { get; set; }
        public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
    }
}
