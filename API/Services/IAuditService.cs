using Domain.Entity;

namespace API.Services
{
    public interface IAuditService
    {
        Task LogAsync(AuditAction action, Guid? actorId, string? entityName, Guid? entityId, string? ipAddress, string? userAgent, object? details);
    }
}
