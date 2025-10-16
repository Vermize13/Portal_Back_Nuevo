using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CreateProjectRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = default!;

        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string Code { get; set; } = default!;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
