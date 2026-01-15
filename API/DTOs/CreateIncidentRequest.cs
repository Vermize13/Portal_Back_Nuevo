using System.ComponentModel.DataAnnotations;
using Domain.Entity;

namespace API.DTOs
{
    public class CreateIncidentRequest
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public Guid SprintId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = default!;

        public string? Description { get; set; }

        public string? TestData { get; set; }

        public string? Evidence { get; set; }

        public string? ExpectedBehavior { get; set; }

        public BugType? BugType { get; set; }

        public IncidentSeverity Severity { get; set; } = IncidentSeverity.Medium;

        public IncidentPriority Priority { get; set; } = IncidentPriority.Should;

        public Guid? AssigneeId { get; set; }

        public decimal? StoryPoints { get; set; }

        public DateOnly? DueDate { get; set; }

        public List<Guid>? LabelIds { get; set; }
    }
}
