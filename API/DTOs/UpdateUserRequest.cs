using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class UpdateUserRequest
    {
        [Required]
        public string Name { get; set; } = default!;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;
        
        [Required]
        public string Username { get; set; } = default!;
        
        /// <summary>
        /// Role code (e.g., "admin", "developer", "tester")
        /// </summary>
        public string? RoleCode { get; set; }
        
        /// <summary>
        /// Role ID (alternative to RoleCode)
        /// </summary>
        public Guid? RoleId { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Optional new password (only set if changing password)
        /// </summary>
        public string? Password { get; set; }
    }
}
