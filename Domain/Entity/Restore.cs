using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entity
{
    public class Restore
    {
        public Guid Id { get; set; }
        public Guid BackupId { get; set; }
        
        [ForeignKey("BackupId")]
        public Backup Backup { get; set; } = default!;
        public Guid RequestedBy { get; set; }
        
        [ForeignKey("RequestedBy")]
        public User Requester { get; set; } = default!;
        public string Status { get; set; } = "running";
        public string? TargetDb { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public string? Notes { get; set; }
    }
}
