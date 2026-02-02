using Microsoft.Extensions.DependencyInjection;
using Domain;

namespace Repository
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(Repositories.IGenericRepository<>), typeof(Repositories.GenericRepository<>));
            services.AddScoped<Repositories.IUserRepository, Repositories.UserRepository>();
            services.AddScoped<Repositories.IProjectRepository, Repositories.ProjectRepository>();
            services.AddScoped<Repositories.IIncidentRepository, Repositories.IncidentRepository>();
            services.AddScoped<Repositories.ISprintRepository, Repositories.SprintRepository>();
            services.AddScoped<Repositories.ILabelRepository, Repositories.LabelRepository>();
            services.AddScoped<Repositories.IAttachmentRepository, Repositories.AttachmentRepository>();
            services.AddScoped<Repositories.IAuditLogRepository, Repositories.AuditLogRepository>();
            services.AddScoped<Repositories.INotificationRepository, Repositories.NotificationRepository>();
            services.AddScoped<Repositories.IBackupRepository, Repositories.BackupRepository>();
            services.AddScoped<Repositories.IRestoreRepository, Repositories.RestoreRepository>();
            services.AddScoped<Repositories.ISystemConfigurationRepository, Repositories.SystemConfigurationRepository>();
            return services;
        }
    }
}
