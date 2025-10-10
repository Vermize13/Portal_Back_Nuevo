using System;

namespace Domain.Entity
{
    public class ProjectMember
    {
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public Guid RoleId { get; set; }
        public Role Role { get; set; } = default!;
        public DateTimeOffset JoinedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
