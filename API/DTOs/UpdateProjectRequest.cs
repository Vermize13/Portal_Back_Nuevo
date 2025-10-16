using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class UpdateProjectRequest
    {
        [StringLength(100, MinimumLength = 1)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }
}
