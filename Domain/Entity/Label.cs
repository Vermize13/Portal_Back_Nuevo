using System;

namespace Domain.Entity
{
    public class Label
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? ColorHex { get; set; }
    }
}
