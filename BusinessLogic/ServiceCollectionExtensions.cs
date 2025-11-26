using BusinessLogic.Services;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using Repository.Repositories;

namespace BusinessLogic
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register services
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProjectMemberService, ProjectMemberService>();

            return services;
        }
    }
}
