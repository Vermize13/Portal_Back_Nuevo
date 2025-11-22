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

        public async Task LogHttpRequestAsync(Guid? actorId, string httpMethod, string httpPath, int statusCode, long durationMs, string? ipAddress, string? userAgent, Guid? requestId)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = AuditAction.HttpRequest,
                ActorId = actorId,
                RequestId = requestId ?? Guid.NewGuid(),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                HttpMethod = httpMethod,
                HttpPath = httpPath,
                HttpStatusCode = statusCode,
                DurationMs = durationMs,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogSqlCommandAsync(string sqlCommand, string? sqlParameters, long durationMs)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = AuditAction.SqlCommand,
                SqlCommand = sqlCommand,
                SqlParameters = sqlParameters,
                DurationMs = durationMs,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}
