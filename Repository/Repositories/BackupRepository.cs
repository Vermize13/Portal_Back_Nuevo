using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public interface IBackupRepository : IGenericRepository<Backup>
    {
        Task<IEnumerable<Backup>> GetByCreatorIdAsync(Guid creatorId);
    }

    public class BackupRepository : GenericRepository<Backup>, IBackupRepository
    {
        public BackupRepository(BugMgrDbContext context) : base(context) { }

        public async Task<IEnumerable<Backup>> GetByCreatorIdAsync(Guid creatorId)
        {
            return await _context.Backups.Where(b => b.CreatedBy == creatorId).ToListAsync();
        }
    }
}
