using Domain.Entity;

namespace WebApi.DTOs
{
    // RF5.2: Filter logs by user, action, and date
    public class AuditFilterRequest
    {
        public Guid? UserId { get; set; }
        public AuditAction? Action { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class AuditLogResponse
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = default!;
        public Guid? ActorId { get; set; }
        public string? ActorUsername { get; set; }
        public string? EntityName { get; set; }
        public Guid? EntityId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? DetailsJson { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class AuditLogPagedResponse
    {
        public List<AuditLogResponse> Logs { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
