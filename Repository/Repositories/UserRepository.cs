using Infrastructure;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdWithRolesAsync(Guid id);
        Task<IEnumerable<User>> GetAllActiveUsersAsync();
        Task<IEnumerable<User>> GetAllUsersWithRolesAsync();
    }

    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(BugMgrDbContext context) : base(context) { }

        public Task<User?> GetByEmailAsync(string email)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public Task<User?> GetByUsernameAsync(string username)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public Task<User?> GetByIdWithRolesAsync(Guid id)
        {
            return _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<User>> GetAllActiveUsersAsync()
        {
            return await _context.Users.Where(u => u.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllUsersWithRolesAsync()
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();
        }
    }
}
