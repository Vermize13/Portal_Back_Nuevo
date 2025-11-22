using System;
using System.Text.Json.Serialization;

namespace Domain.Entity
{
    public class IncidentComment
    {
        public Guid Id { get; set; }
        public Guid IncidentId { get; set; }
        
        [JsonIgnore]
        public Incident Incident { get; set; } = default!;
        public Guid AuthorId { get; set; }
        public User Author { get; set; } = default!;
        public string Body { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? EditedAt { get; set; }
    }
}
