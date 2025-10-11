using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetByActorIdAsync(Guid actorId);
    }

    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(BugMgrDbContext context) : base(context) { }

        public async Task<IEnumerable<AuditLog>> GetByActorIdAsync(Guid actorId)
        {
            return await _context.AuditLogs.Where(a => a.ActorId == actorId).ToListAsync();
        }
    }
}
