using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Domain.Entity;

namespace Domain
{
    public class BugMgrDbContext : DbContext
    {
        public BugMgrDbContext(DbContextOptions<BugMgrDbContext> options) : base(options) {}

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
        public DbSet<Sprint> Sprints => Set<Sprint>();
        public DbSet<Incident> Incidents => Set<Incident>();
        public DbSet<Label> Labels => Set<Label>();
        public DbSet<IncidentLabel> IncidentLabels => Set<IncidentLabel>();
        public DbSet<IncidentComment> IncidentComments => Set<IncidentComment>();
        public DbSet<Attachment> Attachments => Set<Attachment>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<Backup> Backups => Set<Backup>();
        public DbSet<Restore> Restores => Set<Restore>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            NpgsqlModelBuilderExtensions.HasPostgresEnum<IncidentStatus>(b);
            NpgsqlModelBuilderExtensions.HasPostgresEnum<IncidentSeverity>(b);
            NpgsqlModelBuilderExtensions.HasPostgresEnum<IncidentPriority>(b);
            NpgsqlModelBuilderExtensions.HasPostgresEnum<NotificationChannel>(b);
            NpgsqlModelBuilderExtensions.HasPostgresEnum<AuditAction>(b);

            b.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });
            b.Entity<ProjectMember>().HasKey(x => new { x.ProjectId, x.UserId });
            b.Entity<IncidentLabel>().HasKey(x => new { x.IncidentId, x.LabelId });

            b.Entity<Project>()
                .HasIndex(p => p.Code).IsUnique();
            b.Entity<Incident>()
                .HasIndex(i => new { i.ProjectId, i.Code }).IsUnique();
            b.Entity<Project>()
                .HasOne(p => p.Creator)
                .WithMany()
                .HasForeignKey(p => p.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            b.Entity<Attachment>()
                .HasOne(a => a.Uploader)
                .WithMany()
                .HasForeignKey(a => a.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<User>().Property(x => x.Email).IsRequired();
            b.Entity<User>().Property(x => x.Username).IsRequired();
            b.Entity<Project>().Property(x => x.Name).IsRequired();

            base.OnModelCreating(b);
        }
    }
}
