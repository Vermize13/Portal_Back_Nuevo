using Domain.Entity;

namespace WebApi.Services
{
    public interface IAuditService
    {
        Task LogAsync(AuditAction action, Guid? actorId, string? entityName, Guid? entityId, string? ipAddress, string? userAgent, object? details);
    }
}
