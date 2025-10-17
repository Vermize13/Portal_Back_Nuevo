using Infrastructure;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetByActorIdAsync(Guid actorId);
        Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetFilteredAsync(
            Guid? userId, 
            AuditAction? action, 
            DateTime? startDate, 
            DateTime? endDate, 
            int page, 
            int pageSize);
    }

    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(BugMgrDbContext context) : base(context) { }

        public async Task<IEnumerable<AuditLog>> GetByActorIdAsync(Guid actorId)
        {
            return await _context.AuditLogs.Where(a => a.ActorId == actorId).ToListAsync();
        }

        public async Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetFilteredAsync(
            Guid? userId, 
            AuditAction? action, 
            DateTime? startDate, 
            DateTime? endDate, 
            int page, 
            int pageSize)
        {
            var query = _context.AuditLogs
                .Include(a => a.Actor)
                .AsQueryable();

            // Apply filters
            if (userId.HasValue)
                query = query.Where(a => a.ActorId == userId.Value);

            if (action.HasValue)
                query = query.Where(a => a.Action == action.Value);

            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.CreatedAt <= endDate.Value);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }
    }
}
