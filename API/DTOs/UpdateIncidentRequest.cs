using System.ComponentModel.DataAnnotations;
using Domain.Entity;

namespace API.DTOs
{
    public class UpdateIncidentRequest
    {
        [StringLength(200)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? TestData { get; set; }

        public string? Evidence { get; set; }

        public string? ExpectedBehavior { get; set; }

        public BugType? BugType { get; set; }

        public IncidentSeverity? Severity { get; set; }

        public IncidentPriority? Priority { get; set; }

        public IncidentStatus? Status { get; set; }

        public Guid? SprintId { get; set; }

        public Guid? AssigneeId { get; set; }

        public decimal? StoryPoints { get; set; }

        public DateOnly? DueDate { get; set; }
    }
}
