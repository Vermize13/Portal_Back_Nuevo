using System;
using Domain.Entity;

namespace API.DTOs
{
    public class ProjectProgressResponse
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = default!;
        public int TotalSprints { get; set; }
        public int ActiveSprints { get; set; }
        public int ClosedSprints { get; set; }
        public int TotalIncidents { get; set; }
        public int OpenIncidents { get; set; }
        public int InProgressIncidents { get; set; }
        public int ClosedIncidents { get; set; }
        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }
        public decimal CompletionPercentage { get; set; }
    }
}
