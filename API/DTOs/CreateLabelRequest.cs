using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CreateLabelRequest
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; } = default!;

        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "ColorHex must be a valid hex color (e.g., #FF5733)")]
        public string? ColorHex { get; set; }
    }
}
