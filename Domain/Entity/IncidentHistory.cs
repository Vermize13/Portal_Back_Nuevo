using System;

namespace Domain.Entity
{
    public class IncidentHistory
    {
        public Guid Id { get; set; }
        public Guid IncidentId { get; set; }
        public Incident Incident { get; set; } = default!;
        public Guid ChangedBy { get; set; }
        public User ChangedByUser { get; set; } = default!;
        public string FieldName { get; set; } = default!;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTimeOffset ChangedAt { get; set; }
    }
}
