using Domain.Entity;

namespace API.Services
{
    public interface IAuditService
    {
        Task LogAsync(AuditAction action, Guid? actorId, string? entityName, Guid? entityId, string? ipAddress, string? userAgent, object? details);
        Task LogHttpRequestAsync(Guid? actorId, string httpMethod, string httpPath, int statusCode, long durationMs, string? ipAddress, string? userAgent, Guid? requestId);
        Task LogSqlCommandAsync(string sqlCommand, string? sqlParameters, long durationMs);
    }
}
