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
        Task ClearRoleAsync(Guid userId, Guid? actorId = null);
        Task UpdatePasswordAsync(Guid userId, UpdatePasswordDto dto, Guid? actorId = null);
        Task UpdateProfileAsync(Guid userId, UpdateUserDto dto);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly IAuditService _auditService;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork,
            IPasswordService passwordService,
            IAuditService auditService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
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

            // Validate role if provided
            if (dto.RoleId.HasValue)
            {
                var role = await _roleRepository.GetAsync(dto.RoleId.Value);
                if (role == null)
                {
                    throw new InvalidOperationException($"Role with ID {dto.RoleId} not found");
                }
            }

            // Create user with single role
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Username = dto.Username,
                PasswordHash = _passwordService.HashPassword(dto.Password),
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                RoleId = dto.RoleId
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Create,
                actorId,
                nameof(User),
                user.Id,
                new { user.Username, user.Email, user.Name }
            );

            // Get user with role
            var createdUser = await _userRepository.GetByIdWithRoleAsync(user.Id);
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

            var updatedUser = await _userRepository.GetByIdWithRoleAsync(user.Id);
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
            var user = await _userRepository.GetByIdWithRoleAsync(userId);
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

            // Assign single role to user (replaces any existing role)
            user.RoleId = roleId;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Assign,
                actorId,
                nameof(User),
                user.Id,
                new { UserId = userId, RoleId = roleId, RoleName = role.Name }
            );
        }

        public async Task ClearRoleAsync(Guid userId, Guid? actorId = null)
        {
            var user = await _userRepository.GetByIdWithRoleAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            var previousRoleName = user.Role?.Name;

            // Clear the user's role
            user.RoleId = null;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Update,
                actorId,
                nameof(User),
                user.Id,
                new { Action = "ClearRole", UserId = userId, PreviousRole = previousRoleName }
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
                Role = user.Role != null ? new RoleDto
                {
                    Id = user.Role.Id,
                    Code = user.Role.Code,
                    Name = user.Role.Name,
                    Description = user.Role.Description
                } : null
            };
        }
    }
}
