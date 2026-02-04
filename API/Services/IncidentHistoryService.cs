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
                var systemUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
                user = await _context.Users.FirstOrDefaultAsync(u => u.Id == systemUserId);
                
                if (user != null)
                {
                    userId = systemUserId;
                }
                else
                {
                    // If even system user is not found, we cannot create a history record 
                    // without violating FK constraint (assuming ChangedBy is a FK)
                    // We log a warning and skip history logging to prevent crashing the main operation
                    Console.WriteLine($"WARNING: Could not log incident history. User {userId} and System User not found.");
                    return;
                }
            }

            var history = new IncidentHistory
            {
                Id = Guid.NewGuid(),
                IncidentId = incidentId,
                ChangedBy = userId,
                // ChangedByUser is a navigation property, EF will set it based on ChangedBy
                // We don't need to explicitly set it, especially using the dangerous null-forgiving operator
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
