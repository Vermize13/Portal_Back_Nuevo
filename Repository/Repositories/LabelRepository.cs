using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public interface ILabelRepository : IGenericRepository<Label>
    {
        Task<IEnumerable<Label>> GetByProjectIdAsync(Guid projectId);
    }

    public class LabelRepository : GenericRepository<Label>, ILabelRepository
    {
        public LabelRepository(BugMgrDbContext context) : base(context) { }

        public async Task<IEnumerable<Label>> GetByProjectIdAsync(Guid projectId)
        {
            return await _context.Labels.Where(l => l.ProjectId == projectId).ToListAsync();
        }
    }
}
