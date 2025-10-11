using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public interface ISprintRepository : IGenericRepository<Sprint>
    {
        Task<IEnumerable<Sprint>> GetByProjectIdAsync(Guid projectId);
    }

    public class SprintRepository : GenericRepository<Sprint>, ISprintRepository
    {
        public SprintRepository(BugMgrDbContext context) : base(context) { }

        public async Task<IEnumerable<Sprint>> GetByProjectIdAsync(Guid projectId)
        {
            return await _context.Sprints.Where(s => s.ProjectId == projectId).ToListAsync();
        }
    }
}
