using Repository.Repositories;

namespace API.Services
{
    public interface ISystemConfigurationService
    {
        Task<string> GetValueAsync(string key, string defaultValue);
        Task<int> GetIntAsync(string key, int defaultValue);
        Task SetValueAsync(string key, string value, string? description = null, Guid? userId = null);
        Task<Dictionary<string, string>> GetAllPublicConfigsAsync();
    }

    public class SystemConfigurationService : ISystemConfigurationService
    {
        private readonly ISystemConfigurationRepository _repo;
        private readonly ILogger<SystemConfigurationService> _logger;

        public SystemConfigurationService(ISystemConfigurationRepository repo, ILogger<SystemConfigurationService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<string> GetValueAsync(string key, string defaultValue)
        {
            var val = await _repo.GetValueAsync(key);
            return val ?? defaultValue;
        }

        public async Task<int> GetIntAsync(string key, int defaultValue)
        {
            var val = await _repo.GetValueAsync(key);
            if (val != null && int.TryParse(val, out var result))
            {
                return result;
            }
            return defaultValue;
        }

        public async Task SetValueAsync(string key, string value, string? description = null, Guid? userId = null)
        {
            await _repo.SetValueAsync(key, value, description, userId);
            _logger.LogInformation("System config '{Key}' updated to '{Value}' by {UserId}", key, value, userId);
        }

        public async Task<Dictionary<string, string>> GetAllPublicConfigsAsync()
        {
            // For now, return a fixed set or all. Ideally filtering by "public".
            // Since we need max upload size for frontend, we return that.
            // But actually, the frontend fetches "config" via Admin component which probably requires auth.
            // Let's implement getting all raw values from repo?
            // The repo is generic, so we can use GetAllAsync()
            var all = await _repo.GetAllAsync();
            return all.ToDictionary(k => k.Key, v => v.Value);
        }
    }
}
