using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CreateSprintRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = default!;

        [StringLength(500)]
        public string? Goal { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }
    }
}
