using System;

namespace Domain.Entity
{
    public class UserInvitation
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public Guid RoleId { get; set; }
        public Role? Role { get; set; }
        public string Token { get; set; } = default!;
        public DateTimeOffset ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public Guid InvitedByUserId { get; set; }
        public User? InvitedByUser { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UsedAt { get; set; }
    }
}
