using Domain.Entity;

namespace API.DTOs
{
    /// <summary>
    /// Request model for filtering audit logs
    /// </summary>
    public class AuditFilterRequest
    {
        public Guid? UserId { get; set; }
        public AuditAction? Action { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    /// <summary>
    /// Response model for individual audit log
    /// </summary>
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
        
        // HTTP Request specific fields
        public string? HttpMethod { get; set; }
        public string? HttpPath { get; set; }
        public int? HttpStatusCode { get; set; }
        public long? DurationMs { get; set; }
        
        // SQL Command specific fields
        public string? SqlCommand { get; set; }
        public string? SqlParameters { get; set; }
    }

    /// <summary>
    /// Paginated response model for audit logs
    /// </summary>
    public class AuditLogPagedResponse
    {
        public AuditLogResponse[] Logs { get; set; } = Array.Empty<AuditLogResponse>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
