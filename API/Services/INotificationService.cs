using Domain.Entity;

namespace API.Services
{
    public interface INotificationService
    {
        Task NotifyIncidentAssignmentAsync(Incident incident, Guid assigneeId);
        Task NotifyIncidentStatusChangeAsync(Incident incident, IncidentStatus oldStatus, IncidentStatus newStatus);
        Task NotifyIncidentUpdateAsync(Incident incident, IEnumerable<string> changedFields);
        Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody);
    }
}
