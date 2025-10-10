using System;

namespace Domain.Entity
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public AuditAction Action { get; set; }
        public Guid? ActorId { get; set; }
        public User? Actor { get; set; }
        public string? EntityName { get; set; }
        public Guid? EntityId { get; set; }
        public Guid? RequestId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? DetailsJson { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
