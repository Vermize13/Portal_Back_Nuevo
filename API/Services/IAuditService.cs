using Domain.Entity;
using API.DTOs;

namespace API.Services
{
    public interface IAuditService
    {
        Task LogAsync(AuditAction action, Guid? actorId, string? entityName, Guid? entityId, string? ipAddress, string? userAgent, object? details);

        Task LogSqlCommandAsync(string sqlCommand, string? sqlParameters, long durationMs);
        Task<AuditLogPagedResponse> GetFilteredLogsAsync(AuditFilterRequest filter);
        Task<byte[]> ExportLogsAsync(AuditFilterRequest filter);
    }
}
