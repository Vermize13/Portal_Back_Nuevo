using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    public class RegisterRequest
    {
        [Required]
        public string Name { get; set; } = default!;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;
        
        [Required]
        public string Username { get; set; } = default!;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = default!;
    }
}
