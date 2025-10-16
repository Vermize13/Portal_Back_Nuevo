using Infrastructure;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetByCodeAsync(string code);
        Task<IEnumerable<Role>> GetAllActiveRolesAsync();
    }

    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(BugMgrDbContext context) : base(context) { }

        public Task<Role?> GetByCodeAsync(string code)
        {
            return _context.Roles.FirstOrDefaultAsync(r => r.Code == code);
        }

        public async Task<IEnumerable<Role>> GetAllActiveRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }
    }
}
