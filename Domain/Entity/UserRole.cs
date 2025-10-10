using System;

namespace Domain.Entity
{
    public class UserRole
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public Guid RoleId { get; set; }
        public Role Role { get; set; } = default!;
        public DateTimeOffset AssignedAt { get; set; }
    }
}
