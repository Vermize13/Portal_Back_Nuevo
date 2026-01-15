using Domain.Entity;
using Repository.Repositories;
using Microsoft.Extensions.Options;
using API.DTOs;
using Resend;

namespace API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly EmailSettings _emailSettings;
        private readonly IResend _resendClient;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            IOptions<EmailSettings> emailSettings,
            IResend resendClient,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _emailSettings = emailSettings.Value;
            _resendClient = resendClient;
            _logger = logger;
        }

        private string GetIncidentUrl(Guid projectId, Guid incidentId)
        {
            var baseUrl = (_emailSettings.FrontendUrl ?? "http://localhost:4200").TrimEnd('/');
            return $"{baseUrl}/inicio/incidents/{incidentId}";
        }

        public async Task NotifyIncidentAssignmentAsync(Incident incident, Guid assigneeId)
        {
            // Create in-app notification
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

            // Send email notification
            try
            {
                var assignee = await _userRepository.GetAsync(assigneeId);
                if (assignee != null && !string.IsNullOrEmpty(assignee.Email))
                {
                    var incidentUrl = GetIncidentUrl(incident.ProjectId, incident.Id);
                    var subject = "Nueva incidencia asignada";
                    var htmlBody = $@"
                        <h2>Nueva incidencia asignada</h2>
                        <p>Hola {assignee.Name},</p>
                        <p>Se te ha asignado la siguiente incidencia:</p>
                        <ul>
                            <li><strong>Código:</strong> {incident.Code}</li>
                            <li><strong>Título:</strong> {incident.Title}</li>
                            <li><strong>Severidad:</strong> {incident.Severity}</li>
                            <li><strong>Prioridad:</strong> {incident.Priority}</li>
                        </ul>
                        <p>Por favor, revisa los detalles en el sistema:</p>
                        <p><a href=""{incidentUrl}"" style=""display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;"">Ver incidencia</a></p>
                    ";
                    await SendEmailAsync(assignee.Email, assignee.Name, subject, htmlBody);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email notification for incident assignment");
            }
        }

        public async Task NotifyIncidentStatusChangeAsync(Incident incident, IncidentStatus oldStatus, IncidentStatus newStatus)
        {
            if (incident.AssigneeId.HasValue)
            {
                // Create in-app notification
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

                // Send email notification
                try
                {
                    var assignee = await _userRepository.GetAsync(incident.AssigneeId.Value);
                    if (assignee != null && !string.IsNullOrEmpty(assignee.Email))
                    {
                        var incidentUrl = GetIncidentUrl(incident.ProjectId, incident.Id);
                        var subject = "Cambio de estado de incidencia";
                        var htmlBody = $@"
                            <h2>Cambio de estado de incidencia</h2>
                            <p>Hola {assignee.Name},</p>
                            <p>La incidencia <strong>{incident.Code}: {incident.Title}</strong> ha cambiado de estado:</p>
                            <ul>
                                <li><strong>Estado anterior:</strong> {oldStatus}</li>
                                <li><strong>Estado nuevo:</strong> {newStatus}</li>
                            </ul>
                            <p>Revisa los detalles de la incidencia:</p>
                            <p><a href=""{incidentUrl}"" style=""display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;"">Ver incidencia</a></p>
                        ";
                        await SendEmailAsync(assignee.Email, assignee.Name, subject, htmlBody);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email notification for incident status change");
                }
            }
        }

        public async Task NotifyIncidentUpdateAsync(Incident incident, IEnumerable<string> changedFields)
        {
            if (incident.AssigneeId.HasValue)
            {
                var fieldsChanged = string.Join(", ", changedFields);
                
                // Create in-app notification
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = incident.AssigneeId.Value,
                    IncidentId = incident.Id,
                    Channel = NotificationChannel.InApp,
                    Title = "Incidencia actualizada",
                    Message = $"La incidencia {incident.Code} ha sido actualizada. Campos modificados: {fieldsChanged}",
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _notificationRepository.AddAsync(notification);

                // Send email notification
                try
                {
                    var assignee = await _userRepository.GetAsync(incident.AssigneeId.Value);
                    if (assignee != null && !string.IsNullOrEmpty(assignee.Email))
                    {
                        var incidentUrl = GetIncidentUrl(incident.ProjectId, incident.Id);
                        var subject = "Incidencia actualizada";
                        var htmlBody = $@"
                            <h2>Incidencia actualizada</h2>
                            <p>Hola {assignee.Name},</p>
                            <p>La incidencia <strong>{incident.Code}: {incident.Title}</strong> ha sido actualizada.</p>
                            <p><strong>Campos modificados:</strong> {fieldsChanged}</p>
                            <p>Revisa los cambios realizados:</p>
                            <p><a href=""{incidentUrl}"" style=""display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;"">Ver incidencia</a></p>
                        ";
                        await SendEmailAsync(assignee.Email, assignee.Name, subject, htmlBody);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email notification for incident update");
                }
            }
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            try
            {
                var message = new EmailMessage();
                message.From = _emailSettings.FromEmail;
                message.To.Add(toEmail);
                message.Subject = subject;
                message.HtmlBody = htmlBody;

                await _resendClient.EmailSendAsync(message);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }
    }
}
