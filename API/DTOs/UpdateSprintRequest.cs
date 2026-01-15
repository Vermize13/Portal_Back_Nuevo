using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class UpdateSprintRequest
    {
        [StringLength(100, MinimumLength = 1)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Goal { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
    }
}
