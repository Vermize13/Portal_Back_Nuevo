using System;
using System.Collections.Generic;

namespace Domain.Entity
{
    public class Incident
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;
        public Guid? SprintId { get; set; }
        public Sprint? Sprint { get; set; }
        public string Code { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public IncidentSeverity Severity { get; set; } = IncidentSeverity.Medium;
        public IncidentPriority Priority { get; set; } = IncidentPriority.Should;
        public IncidentStatus Status { get; set; } = IncidentStatus.Open;
        public Guid ReporterId { get; set; }
        public User Reporter { get; set; } = default!;
        public Guid? AssigneeId { get; set; }
        public User? Assignee { get; set; }
        public decimal? StoryPoints { get; set; }
        public DateOnly? DueDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
        public ICollection<IncidentLabel> Labels { get; set; } = new List<IncidentLabel>();
        public ICollection<IncidentComment> Comments { get; set; } = new List<IncidentComment>();
    }
}
