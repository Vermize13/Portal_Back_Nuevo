namespace API.DTOs
{
    public class BackupSettings
    {
        public string StoragePath { get; set; } = default!;
        public string PostgresPath { get; set; } = default!;
    }
}
