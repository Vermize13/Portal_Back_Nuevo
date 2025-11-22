using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain;

namespace Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            // Register the SQL command interceptor
            services.AddSingleton<SqlCommandAuditInterceptor>();

            services.AddDbContext<BugMgrDbContext>((serviceProvider, options) =>
            {
                var interceptor = serviceProvider.GetRequiredService<SqlCommandAuditInterceptor>();
                options.UseNpgsql(connectionString)
                       .AddInterceptors(interceptor);
            });

            return services;
        }
    }
}
