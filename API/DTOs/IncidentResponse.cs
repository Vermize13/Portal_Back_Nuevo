using Domain.Entity;

namespace API.DTOs
{
    public class IncidentResponse
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = default!;
        public Guid? SprintId { get; set; }
        public string Code { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public IncidentSeverity Severity { get; set; }
        public IncidentPriority Priority { get; set; }
        public IncidentStatus Status { get; set; }
        public Guid ReporterId { get; set; }
        public string ReporterName { get; set; } = default!;
        public Guid? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public decimal? StoryPoints { get; set; }
        public DateOnly? DueDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
        public LabelInfo[] Labels { get; set; } = Array.Empty<LabelInfo>();
        public int CommentCount { get; set; }
        public int AttachmentCount { get; set; }
    }

    public class LabelInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? ColorHex { get; set; }
    }
}
