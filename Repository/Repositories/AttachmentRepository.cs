using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public interface IAttachmentRepository : IGenericRepository<Attachment>
    {
        Task<IEnumerable<Attachment>> GetByIncidentIdAsync(Guid incidentId);
    }

    public class AttachmentRepository : GenericRepository<Attachment>, IAttachmentRepository
    {
        public AttachmentRepository(BugMgrDbContext context) : base(context) { }

        public async Task<IEnumerable<Attachment>> GetByIncidentIdAsync(Guid incidentId)
        {
            return await _context.Attachments.Where(a => a.IncidentId == incidentId).ToListAsync();
        }
    }
}
