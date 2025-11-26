using Domain.Entity;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public interface IIncidentHistoryService
    {
        Task LogAsync(Guid incidentId, Guid userId, string fieldName, string? oldValue, string? newValue);
    }

    public class IncidentHistoryService : IIncidentHistoryService
    {
        private readonly BugMgrDbContext _context;

        public IncidentHistoryService(BugMgrDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(Guid incidentId, Guid userId, string fieldName, string? oldValue, string? newValue)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                // Use system user if not found
                userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
                user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            }

            var history = new IncidentHistory
            {
                Id = Guid.NewGuid(),
                IncidentId = incidentId,
                ChangedBy = userId,
                ChangedByUser = user!,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                ChangedAt = DateTimeOffset.UtcNow
            };

            _context.IncidentHistories.Add(history);
            await _context.SaveChangesAsync();
        }
    }
}
