using Domain.Entity;
using Infrastructure;
using Newtonsoft.Json;

namespace API.Services
{
    public class AuditService : IAuditService
    {
        private readonly BugMgrDbContext _context;

        public AuditService(BugMgrDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(AuditAction action, Guid? actorId, string? entityName, Guid? entityId, string? ipAddress, string? userAgent, object? details)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = action,
                ActorId = actorId,
                EntityName = entityName,
                EntityId = entityId,
                RequestId = Guid.NewGuid(),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                DetailsJson = details != null ? JsonConvert.SerializeObject(details) : null,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}
