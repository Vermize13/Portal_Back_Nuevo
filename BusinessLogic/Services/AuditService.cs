using Domain.Entity;
using Repository.Repositories;
using Repository;
using Newtonsoft.Json;

namespace BusinessLogic.Services
{
    public interface IAuditService
    {
        Task LogAsync(AuditAction action, Guid? actorId, string? entityName, Guid? entityId, object? details = null, string? ipAddress = null, string? userAgent = null);
    }

    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AuditService(IAuditLogRepository auditLogRepository, IUnitOfWork unitOfWork)
        {
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task LogAsync(AuditAction action, Guid? actorId, string? entityName, Guid? entityId, object? details = null, string? ipAddress = null, string? userAgent = null)
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

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
