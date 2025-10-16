using Domain.Entity;
using Repository.Repositories;

namespace API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task NotifyIncidentAssignmentAsync(Incident incident, Guid assigneeId)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = assigneeId,
                IncidentId = incident.Id,
                Channel = NotificationChannel.InApp,
                Title = "Nueva incidencia asignada",
                Message = $"Se te ha asignado la incidencia {incident.Code}: {incident.Title}",
                IsRead = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _notificationRepository.AddAsync(notification);
        }

        public async Task NotifyIncidentStatusChangeAsync(Incident incident, IncidentStatus oldStatus, IncidentStatus newStatus)
        {
            if (incident.AssigneeId.HasValue)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = incident.AssigneeId.Value,
                    IncidentId = incident.Id,
                    Channel = NotificationChannel.InApp,
                    Title = "Cambio de estado de incidencia",
                    Message = $"La incidencia {incident.Code} ha cambiado de {oldStatus} a {newStatus}",
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _notificationRepository.AddAsync(notification);
            }
        }
    }
}
