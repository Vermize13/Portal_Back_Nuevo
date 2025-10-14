using Infrastructure;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
    }

    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(BugMgrDbContext context) : base(context) { }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Notifications.Where(n => n.UserId == userId).ToListAsync();
        }
    }
}
