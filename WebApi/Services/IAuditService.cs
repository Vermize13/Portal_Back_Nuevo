using Domain.Entity;
using WebApi.DTOs;

namespace WebApi.Services
{
    public interface IAuditService
    {
        Task LogAsync(AuditAction action, Guid? actorId, string? entityName, Guid? entityId, string? ipAddress, string? userAgent, object? details);
        Task<AuditLogPagedResponse> GetFilteredLogsAsync(AuditFilterRequest filter);
        Task<byte[]> ExportLogsAsync(AuditFilterRequest filter);
    }
}
