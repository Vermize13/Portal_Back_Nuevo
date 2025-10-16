using Domain.Entity;

namespace API.Services
{
    public interface INotificationService
    {
        Task NotifyIncidentAssignmentAsync(Incident incident, Guid assigneeId);
        Task NotifyIncidentStatusChangeAsync(Incident incident, IncidentStatus oldStatus, IncidentStatus newStatus);
    }
}
