using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class AddProjectMemberRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid RoleId { get; set; }
    }
}
