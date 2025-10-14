using Infrastructure;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
        Task<Project?> GetByCodeAsync(string code);
    }

    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        public ProjectRepository(BugMgrDbContext context) : base(context) { }

        public Task<Project?> GetByCodeAsync(string code)
        {
            return _context.Projects.FirstOrDefaultAsync(p => p.Code == code);
        }
    }
}
