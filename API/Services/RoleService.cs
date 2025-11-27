using Domain.Entity;
using Repository.Repositories;

namespace API.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(Guid id);
        Task<Role?> GetRoleByCodeAsync(string code);
    }

    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _roleRepository.GetAllActiveRolesAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(Guid id)
        {
            return await _roleRepository.GetAsync(id);
        }

        public async Task<Role?> GetRoleByCodeAsync(string code)
        {
            return await _roleRepository.GetByCodeAsync(code);
        }
    }
}
