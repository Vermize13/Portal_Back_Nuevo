using System;

namespace Domain.Entity
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        
        // Single role per user (1:N relationship)
        public Guid? RoleId { get; set; }
        public Role? Role { get; set; }

        public string? ResetToken { get; set; }
        public DateTimeOffset? ResetTokenExpires { get; set; }
    }
}
