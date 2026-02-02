using System.ComponentModel.DataAnnotations;

namespace Domain.Entity
{
    public class SystemConfiguration
    {
        [Key]
        public string Key { get; set; } = string.Empty;
        
        public string Value { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public DateTimeOffset UpdatedAt { get; set; }
        
        public Guid? UpdatedBy { get; set; }
        
        public virtual User? UpdatedByUser { get; set; }
    }
}
