using Domain.Entity;
using Repository;
using Repository.Repositories;
using BusinessLogic.DTOs;
using Mapster;

namespace BusinessLogic.Services
{
    public interface IUserService
    {
        Task<UserDto> CreateUserAsync(CreateUserDto dto, Guid? actorId = null);
        Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserDto dto, Guid? actorId = null);
        Task DeleteUserAsync(Guid userId, Guid? actorId = null);
        Task DeactivateUserAsync(Guid userId, Guid? actorId = null);
        Task ActivateUserAsync(Guid userId, Guid? actorId = null);
        Task<UserDto?> GetUserByIdAsync(Guid userId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<IEnumerable<UserDto>> GetActiveUsersAsync();
        Task AssignRoleAsync(Guid userId, Guid roleId, Guid? actorId = null);
        Task RemoveRoleAsync(Guid userId, Guid roleId, Guid? actorId = null);
        Task UpdatePasswordAsync(Guid userId, UpdatePasswordDto dto, Guid? actorId = null);
        Task UpdateProfileAsync(Guid userId, UpdateUserDto dto);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly IAuditService _auditService;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IUnitOfWork unitOfWork,
            IPasswordService passwordService,
            IAuditService auditService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _auditService = auditService;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto, Guid? actorId = null)
        {
            // Validate unique email and username
            var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingEmail != null)
            {
                throw new InvalidOperationException("Email already exists");
            }

            var existingUsername = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existingUsername != null)
            {
                throw new InvalidOperationException("Username already exists");
            }

            // Create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Username = dto.Username,
                PasswordHash = _passwordService.HashPassword(dto.Password),
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Assign roles
            foreach (var roleId in dto.RoleIds)
            {
                var role = await _roleRepository.GetAsync(roleId);
                if (role == null)
                {
                    throw new InvalidOperationException($"Role with ID {roleId} not found");
                }

                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId,
                    AssignedAt = DateTimeOffset.UtcNow
                };

                await _userRoleRepository.AddAsync(userRole);
            }

            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Create,
                actorId,
                nameof(User),
                user.Id,
                new { user.Username, user.Email, user.Name }
            );

            // Get user with roles
            var createdUser = await _userRepository.GetByIdWithRolesAsync(user.Id);
            return MapUserToDto(createdUser!);
        }

        public async Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserDto dto, Guid? actorId = null)
        {
            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Validate unique email and username if changed
            if (user.Email != dto.Email)
            {
                var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
                if (existingEmail != null)
                {
                    throw new InvalidOperationException("Email already exists");
                }
            }

            if (user.Username != dto.Username)
            {
                var existingUsername = await _userRepository.GetByUsernameAsync(dto.Username);
                if (existingUsername != null)
                {
                    throw new InvalidOperationException("Username already exists");
                }
            }

            user.Name = dto.Name;
            user.Email = dto.Email;
            user.Username = dto.Username;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Update,
                actorId,
                nameof(User),
                user.Id,
                new { user.Username, user.Email, user.Name }
            );

            var updatedUser = await _userRepository.GetByIdWithRolesAsync(user.Id);
            return MapUserToDto(updatedUser!);
        }

        public async Task DeleteUserAsync(Guid userId, Guid? actorId = null)
        {
            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            _userRepository.Remove(user);
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Delete,
                actorId,
                nameof(User),
                user.Id,
                new { user.Username, user.Email }
            );
        }

        public async Task DeactivateUserAsync(Guid userId, Guid? actorId = null)
        {
            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            user.IsActive = false;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Update,
                actorId,
                nameof(User),
                user.Id,
                new { Action = "Deactivate", user.Username }
            );
        }

        public async Task ActivateUserAsync(Guid userId, Guid? actorId = null)
        {
            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            user.IsActive = true;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Update,
                actorId,
                nameof(User),
                user.Id,
                new { Action = "Activate", user.Username }
            );
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(userId);
            return user != null ? MapUserToDto(user) : null;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersWithRolesAsync();
            return users.Select(MapUserToDto);
        }

        public async Task<IEnumerable<UserDto>> GetActiveUsersAsync()
        {
            var users = await _userRepository.GetAllUsersWithRolesAsync();
            return users.Where(u => u.IsActive).Select(MapUserToDto);
        }

        public async Task AssignRoleAsync(Guid userId, Guid roleId, Guid? actorId = null)
        {
            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            var role = await _roleRepository.GetAsync(roleId);
            if (role == null)
            {
                throw new InvalidOperationException("Role not found");
            }

            var exists = await _userRoleRepository.ExistsAsync(userId, roleId);
            if (exists)
            {
                throw new InvalidOperationException("User already has this role");
            }

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                AssignedAt = DateTimeOffset.UtcNow
            };

            await _userRoleRepository.AddAsync(userRole);
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Assign,
                actorId,
                nameof(UserRole),
                null,
                new { UserId = userId, RoleId = roleId, RoleName = role.Name }
            );
        }

        public async Task RemoveRoleAsync(Guid userId, Guid roleId, Guid? actorId = null)
        {
            var userRole = await _userRoleRepository.GetByUserAndRoleAsync(userId, roleId);
            if (userRole == null)
            {
                throw new InvalidOperationException("User role not found");
            }

            _userRoleRepository.Remove(userRole);
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Update,
                actorId,
                nameof(UserRole),
                null,
                new { Action = "RemoveRole", UserId = userId, RoleId = roleId }
            );
        }

        public async Task UpdatePasswordAsync(Guid userId, UpdatePasswordDto dto, Guid? actorId = null)
        {
            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Verify current password
            if (!_passwordService.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            {
                throw new InvalidOperationException("Current password is incorrect");
            }

            user.PasswordHash = _passwordService.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTimeOffset.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Update,
                actorId ?? userId,
                nameof(User),
                user.Id,
                new { Action = "PasswordChange", user.Username }
            );
        }

        public async Task UpdateProfileAsync(Guid userId, UpdateUserDto dto)
        {
            await UpdateUserAsync(userId, dto, userId);
        }

        private UserDto MapUserToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Username = user.Username,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.UserRoles.Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Code = ur.Role.Code,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description
                }).ToList()
            };
        }
    }
}
