using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public interface IIncidentRepository : IGenericRepository<Incident>
    {
        Task<IEnumerable<Incident>> GetByProjectIdAsync(Guid projectId);
    }

    public class IncidentRepository : GenericRepository<Incident>, IIncidentRepository
    {
        public IncidentRepository(BugMgrDbContext context) : base(context) { }

        public async Task<IEnumerable<Incident>> GetByProjectIdAsync(Guid projectId)
        {
            return await _context.Incidents.Where(i => i.ProjectId == projectId).ToListAsync();
        }
    }
}
