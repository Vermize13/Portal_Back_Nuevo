using Microsoft.AspNetCore.Mvc;
using Domain.Entity;
using Repository.Repositories;
using Repository;
using API.DTOs;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork,
            ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los usuarios del sistema
        /// </summary>
        /// <returns>Lista de usuarios</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
        {
            var users = await _userRepository.GetAllUsersWithRolesAsync();

            var response = users.Select(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Username = u.Username,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                Role = u.Role != null ? new RoleInfo { Id = u.Role.Id, Code = u.Role.Code, Name = u.Role.Name } : null,
            }).ToArray();

            return Ok(response);
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Usuario solicitado</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponse>> GetUser(Guid id)
        {
            var user = await _userRepository.GetByIdWithRoleAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var response = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Username = user.Username,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Role = user.Role != null ? new RoleInfo { Id = user.Role.Id, Code = user.Role.Code, Name = user.Role.Name } : null
            };

            return Ok(response);
        }

        /// <summary>
        /// Obtiene un usuario por su email
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <returns>Usuario solicitado</returns>
        [HttpGet("by-email/{email}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponse>> GetUserByEmail(string email)
        {
            var userBasic = await _userRepository.GetByEmailAsync(email);
            if (userBasic == null)
            {
                return NotFound();
            }

            var user = await _userRepository.GetByIdWithRoleAsync(userBasic.Id);
            if (user == null)
                return NotFound();

            var response = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Username = user.Username,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Role = user.Role != null ? new RoleInfo { Id = user.Role.Id, Code = user.Role.Code, Name = user.Role.Name } : null
            };

            return Ok(response);
        }

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="request">Datos de actualización</param>
        /// <returns>Usuario actualizado</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserResponse>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdWithRoleAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Update basic fields
            user.Name = request.Name;
            user.Email = request.Email;
            user.Username = request.Username;
            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            // Update role if provided
            if (request.RoleId.HasValue)
            {
                var role = await _roleRepository.GetAsync(request.RoleId.Value);
                if (role == null)
                {
                    return BadRequest("Role not found");
                }
                user.RoleId = role.Id;
                user.Role = role;
            }
            else if (!string.IsNullOrEmpty(request.RoleCode))
            {
                var role = await _roleRepository.GetByCodeAsync(request.RoleCode);
                if (role == null)
                {
                    return BadRequest($"Role with code '{request.RoleCode}' not found");
                }
                user.RoleId = role.Id;
                user.Role = role;
            }

            // Update password if provided
            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            var response = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Username = user.Username,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Role = user.Role != null ? new RoleInfo { Id = user.Role.Id, Code = user.Role.Code, Name = user.Role.Name } : null
            };

            return Ok(response);
        }

        /// <summary>
        /// Elimina un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>NoContent si se eliminó correctamente</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _userRepository.GetAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _userRepository.Remove(user);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
