using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entity
{
    public class Backup
    {
        public Guid Id { get; set; }
        public Guid CreatedBy { get; set; }
        
        [ForeignKey("CreatedBy")]
        public User Creator { get; set; } = default!;
        public string StoragePath { get; set; } = default!;
        public string Strategy { get; set; } = default!;
        public long? SizeBytes { get; set; }
        public string Status { get; set; } = "completed";
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public string? Notes { get; set; }
    }
}
