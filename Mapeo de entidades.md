Mapeo de entidades en C# (EF Core + Npgsql)

// Enums mapeados a PostgreSQL enums (recomendado con Npgsql)

public enum IncidentStatus { Open, InProgress, Resolved, Closed, Rejected, Duplicated }

public enum IncidentSeverity { Low, Medium, High, Critical }

public enum IncidentPriority { Wont, Could, Should, Must } // 'Wont' para evitar apóstrofe

public enum NotificationChannel { InApp, Email, Webhook }

public enum AuditAction { Create, Update, Delete, Login, Logout, Assign, Transition, Backup, Restore, Upload, Download }

public class User {

`    `public Guid Id { get; set; }

`    `public string Name { get; set; } = default!;

`    `public string Email { get; set; } = default!;

`    `public string Username { get; set; } = default!;

`    `public string PasswordHash { get; set; } = default!;

`    `public bool IsActive { get; set; } = true;

`    `public DateTimeOffset CreatedAt { get; set; }

`    `public DateTimeOffset UpdatedAt { get; set; }

`    `public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

}

public class Role {

`    `public Guid Id { get; set; }

`    `public string Code { get; set; } = default!;

`    `public string Name { get; set; } = default!;

`    `public string? Description { get; set; }

}

public class UserRole {

`    `public Guid UserId { get; set; }

`    `public User User { get; set; } = default!;

`    `public Guid RoleId { get; set; }

`    `public Role Role { get; set; } = default!;

`    `public DateTimeOffset AssignedAt { get; set; }

}

public class Project {

`    `public Guid Id { get; set; }

`    `public string Name { get; set; } = default!;

`    `public string Code { get; set; } = default!;

`    `public string? Description { get; set; }

`    `public bool IsActive { get; set; } = true;

`    `public Guid CreatedBy { get; set; }

`    `public User Creator { get; set; } = default!;

`    `public DateTimeOffset CreatedAt { get; set; }

`    `public DateTimeOffset UpdatedAt { get; set; }

`    `public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();

}

public class ProjectMember {

`    `public Guid ProjectId { get; set; }

`    `public Project Project { get; set; } = default!;

`    `public Guid UserId { get; set; }

`    `public User User { get; set; } = default!;

`    `public Guid RoleId { get; set; }

`    `public Role Role { get; set; } = default!;

`    `public DateTimeOffset JoinedAt { get; set; }

`    `public bool IsActive { get; set; } = true;

}

public class Sprint {

`    `public Guid Id { get; set; }

`    `public Guid ProjectId { get; set; }

`    `public Project Project { get; set; } = default!;

`    `public string Name { get; set; } = default!;

`    `public string? Goal { get; set; }

`    `public DateOnly StartDate { get; set; }

`    `public DateOnly EndDate { get; set; }

`    `public bool IsClosed { get; set; }

`    `public DateTimeOffset CreatedAt { get; set; }

}

public class Incident {

`    `public Guid Id { get; set; }

`    `public Guid ProjectId { get; set; }

`    `public Project Project { get; set; } = default!;

`    `public Guid? SprintId { get; set; }

`    `public Sprint? Sprint { get; set; }

`    `public string Code { get; set; } = default!;

`    `public string Title { get; set; } = default!;

`    `public string? Description { get; set; }

`    `public IncidentSeverity Severity { get; set; } = IncidentSeverity.Medium;

`    `public IncidentPriority Priority { get; set; } = IncidentPriority.Should;

`    `public IncidentStatus Status { get; set; } = IncidentStatus.Open;

`    `public Guid ReporterId { get; set; }

`    `public User Reporter { get; set; } = default!;

`    `public Guid? AssigneeId { get; set; }

`    `public User? Assignee { get; set; }

`    `public decimal? StoryPoints { get; set; }

`    `public DateOnly? DueDate { get; set; }

`    `public DateTimeOffset CreatedAt { get; set; }

`    `public DateTimeOffset UpdatedAt { get; set; }

`    `public DateTimeOffset? ClosedAt { get; set; }

`    `public ICollection<IncidentLabel> Labels { get; set; } = new List<IncidentLabel>();

`    `public ICollection<IncidentComment> Comments { get; set; } = new List<IncidentComment>();

}

public class Label {

`    `public Guid Id { get; set; }

`    `public Guid ProjectId { get; set; }

`    `public Project Project { get; set; } = default!;

`    `public string Name { get; set; } = default!;

`    `public string? ColorHex { get; set; }

}

public class IncidentLabel {

`    `public Guid IncidentId { get; set; }

`    `public Incident Incident { get; set; } = default!;

`    `public Guid LabelId { get; set; }

`    `public Label Label { get; set; } = default!;

}

public class IncidentComment {

`    `public Guid Id { get; set; }

`    `public Guid IncidentId { get; set; }

`    `public Incident Incident { get; set; } = default!;

`    `public Guid AuthorId { get; set; }

`    `public User Author { get; set; } = default!;

`    `public string Body { get; set; } = default!;

`    `public DateTimeOffset CreatedAt { get; set; }

`    `public DateTimeOffset? EditedAt { get; set; }

}

public class Attachment {

`    `public Guid Id { get; set; }

`    `public Guid IncidentId { get; set; }

`    `public Incident Incident { get; set; } = default!;

`    `public Guid UploadedBy { get; set; }

`    `public User Uploader { get; set; } = default!;

`    `public string FileName { get; set; } = default!;

`    `public string StoragePath { get; set; } = default!;

`    `apublic string MimeType { get; set; } = default!;

`    `public long FileSizeBytes { get; set; }

`    `public string? Sha256Checksum { get; set; }

`    `public DateTimeOffset UploadedAt { get; set; }

}

public class Notification {

`    `public Guid Id { get; set; }

`    `public Guid UserId { get; set; }

`    `public User User { get; set; } = default!;

`    `public Guid? IncidentId { get; set; }

`    `public Incident? Incident { get; set; }

`    `public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;

`    `public string Title { get; set; } = default!;

`    `public string Message { get; set; } = default!;

`    `public bool IsRead { get; set; }

`    `public DateTimeOffset CreatedAt { get; set; }

`    `public DateTimeOffset? ReadAt { get; set; }

}

public class AuditLog {

`    `public Guid Id { get; set; }

`    `public AuditAction Action { get; set; }

`    `public Guid? ActorId { get; set; }

`    `public User? Actor { get; set; }

`    `public string? EntityName { get; set; }

`    `public Guid? EntityId { get; set; }

`    `public Guid? RequestId { get; set; }

`    `public string? IpAddress { get; set; }

`    `public string? UserAgent { get; set; }

`    `public string? DetailsJson { get; set; }

`    `public DateTimeOffset CreatedAt { get; set; }

}

public class Backup {

`    `public Guid Id { get; set; }

`    `public Guid CreatedBy { get; set; }

`    `public User Creator { get; set; } = default!;

`    `public string StoragePath { get; set; } = default!;

`    `public string Strategy { get; set; } = default!;

`    `public long? SizeBytes { get; set; }

`    `public string Status { get; set; } = "completed";

`    `public DateTimeOffset StartedAt { get; set; }

`    `public DateTimeOffset? FinishedAt { get; set; }

`    `public string? Notes { get; set; }

}

public class Restore {

`    `public Guid Id { get; set; }

`    `public Guid BackupId { get; set; }

`    `public Backup Backup { get; set; } = default!;

`    `public Guid RequestedBy { get; set; }

`    `public User Requester { get; set; } = default!;

`    `public string Status { get; set; } = "running";

`    `public string? TargetDb { get; set; }

`    `public DateTimeOffset StartedAt { get; set; }

`    `public DateTimeOffset? FinishedAt { get; set; }

`    `public string? Notes { get; set; }

}



// DbContext (configuración esencial)

public class BugMgrDbContext : DbContext

{

`    `public BugMgrDbContext(DbContextOptions<BugMgrDbContext> options) : base(options) {}

`    `public DbSet<User> Users => Set<User>();

`    `public DbSet<Role> Roles => Set<Role>();

`    `public DbSet<UserRole> UserRoles => Set<UserRole>();

`    `public DbSet<Project> Projects => Set<Project>();

`    `public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

`    `public DbSet<Sprint> Sprints => Set<Sprint>();

`    `public DbSet<Incident> Incidents => Set<Incident>();

`    `public DbSet<Label> Labels => Set<Label>();

`    `public DbSet<IncidentLabel> IncidentLabels => Set<IncidentLabel>();

`    `public DbSet<IncidentComment> IncidentComments => Set<IncidentComment>();

`    `public DbSet<Attachment> Attachments => Set<Attachment>();

`    `public DbSet<Notification> Notifications => Set<Notification>();

`    `public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

`    `public DbSet<Backup> Backups => Set<Backup>();

`    `public DbSet<Restore> Restores => Set<Restore>();

`    `protected override void OnModelCreating(ModelBuilder b)

`    `{

`        `// Mapear enums de C# a enums de PostgreSQL

`        `NpgsqlModelBuilderExtensions.HasPostgresEnum<IncidentStatus>(b);

`        `NpgsqlModelBuilderExtensions.HasPostgresEnum<IncidentSeverity>(b);

`        `NpgsqlModelBuilderExtensions.HasPostgresEnum<IncidentPriority>(b);

`        `NpgsqlModelBuilderExtensions.HasPostgresEnum<NotificationChannel>(b);

`        `NpgsqlModelBuilderExtensions.HasPostgresEnum<AuditAction>(b);

`        `b.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });

`        `b.Entity<ProjectMember>().HasKey(x => new { x.ProjectId, x.UserId });

`        `b.Entity<IncidentLabel>().HasKey(x => new { x.IncidentId, x.LabelId });

`        `b.Entity<Project>()

.HasIndex(p => p.Code).IsUnique();

`        `b.Entity<Incident>()

.HasIndex(i => new { i.ProjectId, i.Code }).IsUnique();

`        `b.Entity<Project>()

.HasOne(p => p.Creator)

.WithMany()

.HasForeignKey(p => p.CreatedBy)

.OnDelete(DeleteBehavior.Restrict);

`        `b.Entity<Attachment>()

.HasOne(a => a.Uploader)

.WithMany()

.HasForeignKey(a => a.UploadedBy)

.OnDelete(DeleteBehavior.Restrict);

`        `// Campos de texto requeridos

`        `b.Entity<User>().Property(x => x.Email).IsRequired();

`        `b.Entity<User>().Property(x => x.Username).IsRequired();

`        `b.Entity<Project>().Property(x => x.Name).IsRequired();

`        `base.OnModelCreating(b);

`    `}

}
