using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
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
    }
}
