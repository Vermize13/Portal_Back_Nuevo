using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class AddCommentRequest
    {
        [Required]
        public string Body { get; set; } = default!;
    }
}
