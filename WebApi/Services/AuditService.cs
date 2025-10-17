using Domain.Entity;
using Infrastructure;
using Newtonsoft.Json;
using Repository.Repositories;
using System.Text;
using WebApi.DTOs;

namespace WebApi.Services
{
    public class AuditService : IAuditService
    {
        private readonly BugMgrDbContext _context;
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditService(BugMgrDbContext context, IAuditLogRepository auditLogRepository)
        {
            _context = context;
            _auditLogRepository = auditLogRepository;
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

        // RF5.2: Get filtered audit logs with pagination
        public async Task<AuditLogPagedResponse> GetFilteredLogsAsync(AuditFilterRequest filter)
        {
            var (logs, totalCount) = await _auditLogRepository.GetFilteredAsync(
                filter.UserId,
                filter.Action,
                filter.StartDate,
                filter.EndDate,
                filter.Page,
                filter.PageSize
            );

            var logResponses = logs.Select(log => new AuditLogResponse
            {
                Id = log.Id,
                Action = log.Action.ToString(),
                ActorId = log.ActorId,
                ActorUsername = log.Actor?.Username,
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                DetailsJson = log.DetailsJson,
                CreatedAt = log.CreatedAt
            }).ToList();

            return new AuditLogPagedResponse
            {
                Logs = logResponses,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }

        // RF5.3: Export audit logs as CSV
        public async Task<byte[]> ExportLogsAsync(AuditFilterRequest filter)
        {
            // Remove pagination for export (get all matching records)
            var (logs, _) = await _auditLogRepository.GetFilteredAsync(
                filter.UserId,
                filter.Action,
                filter.StartDate,
                filter.EndDate,
                1,
                int.MaxValue
            );

            var csv = new StringBuilder();
            csv.AppendLine("Id,Action,ActorId,ActorUsername,EntityName,EntityId,IpAddress,UserAgent,CreatedAt,Details");

            foreach (var log in logs)
            {
                csv.AppendLine($"{log.Id}," +
                              $"{log.Action}," +
                              $"{log.ActorId}," +
                              $"\"{log.Actor?.Username ?? ""}\"," +
                              $"\"{log.EntityName ?? ""}\"," +
                              $"{log.EntityId}," +
                              $"\"{log.IpAddress ?? ""}\"," +
                              $"\"{log.UserAgent?.Replace("\"", "\"\"") ?? ""}\"," +
                              $"{log.CreatedAt:yyyy-MM-dd HH:mm:ss}," +
                              $"\"{log.DetailsJson?.Replace("\"", "\"\"") ?? ""}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }
    }
}
