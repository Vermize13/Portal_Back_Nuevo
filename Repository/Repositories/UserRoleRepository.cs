using Infrastructure;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public interface IUserRoleRepository
    {
        Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId);
        Task<UserRole?> GetByUserAndRoleAsync(Guid userId, Guid roleId);
        Task AddAsync(UserRole userRole);
        void Remove(UserRole userRole);
        Task<bool> ExistsAsync(Guid userId, Guid roleId);
    }

    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly BugMgrDbContext _context;

        public UserRoleRepository(BugMgrDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();
        }

        public Task<UserRole?> GetByUserAndRoleAsync(Guid userId, Guid roleId)
        {
            return _context.UserRoles
                .Include(ur => ur.Role)
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }

        public Task AddAsync(UserRole userRole)
        {
            _context.UserRoles.Add(userRole);
            return Task.CompletedTask;
        }

        public void Remove(UserRole userRole)
        {
            _context.UserRoles.Remove(userRole);
        }

        public Task<bool> ExistsAsync(Guid userId, Guid roleId)
        {
            return _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }
    }
}
