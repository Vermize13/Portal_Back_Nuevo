using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = default!;
        
        [Required]
        public string Password { get; set; } = default!;
    }
}
