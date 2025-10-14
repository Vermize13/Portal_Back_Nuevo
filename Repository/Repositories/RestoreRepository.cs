using Infrastructure;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public interface IRestoreRepository : IGenericRepository<Restore>
    {
        Task<IEnumerable<Restore>> GetByBackupIdAsync(Guid backupId);
    }

    public class RestoreRepository : GenericRepository<Restore>, IRestoreRepository
    {
        public RestoreRepository(BugMgrDbContext context) : base(context) { }

        public async Task<IEnumerable<Restore>> GetByBackupIdAsync(Guid backupId)
        {
            return await _context.Restores.Where(r => r.BackupId == backupId).ToListAsync();
        }
    }
}
