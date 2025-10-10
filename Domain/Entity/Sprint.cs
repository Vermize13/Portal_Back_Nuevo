using System;

namespace Domain.Entity
{
    public class Sprint
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Goal { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsClosed { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
