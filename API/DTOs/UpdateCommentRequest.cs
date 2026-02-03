using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class UpdateCommentRequest
    {
        [Required]
        public string Body { get; set; } = default!;
    }
}
