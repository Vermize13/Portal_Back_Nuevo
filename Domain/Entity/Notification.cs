using System;

namespace Domain.Entity
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public Guid? IncidentId { get; set; }
        public Incident? Incident { get; set; }
        public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;
        public bool IsRead { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ReadAt { get; set; }
    }
}
