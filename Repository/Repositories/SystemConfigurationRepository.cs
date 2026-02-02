using Infrastructure;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public interface ISystemConfigurationRepository : IGenericRepository<SystemConfiguration>
    {
        Task<string?> GetValueAsync(string key);
        Task SetValueAsync(string key, string value, string? description, Guid? userId);
    }

    public class SystemConfigurationRepository : GenericRepository<SystemConfiguration>, ISystemConfigurationRepository
    {
        public SystemConfigurationRepository(BugMgrDbContext context) : base(context) { }

        public async Task<string?> GetValueAsync(string key)
        {
            var config = await _context.SystemConfigurations.FindAsync(key);
            return config?.Value;
        }

        public async Task SetValueAsync(string key, string value, string? description, Guid? userId)
        {
            var config = await _context.SystemConfigurations.FindAsync(key);
            if (config == null)
            {
                config = new SystemConfiguration
                {
                    Key = key,
                    Value = value,
                    Description = description,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    UpdatedBy = userId
                };
                await _context.SystemConfigurations.AddAsync(config);
            }
            else
            {
                config.Value = value;
                if (description != null) config.Description = description;
                config.UpdatedAt = DateTimeOffset.UtcNow;
                config.UpdatedBy = userId;
                _context.SystemConfigurations.Update(config);
            }
            // Note: SaveChanges is typically called by UnitOfWork, but GenericRepository might handle it or we might need to call it if not using UoW for this specific op.
            // Following the pattern, we usually let UoW handle it, but here SetValueAsync acts like a command.
            // However, looking at ProjectRepository.AddMemberAsync, it calls SaveChangesAsync. So I will too.
            await _context.SaveChangesAsync();
        }
    }
}
