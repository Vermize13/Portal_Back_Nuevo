using Microsoft.AspNetCore.Mvc;
using Domain.Entity;
using Repository.Repositories;
using API.DTOs;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
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
                Roles = u.UserRoles?.Select(ur => new RoleInfo { Id = ur.Role.Id, Code = ur.Role.Code, Name = ur.Role.Name }).ToArray() ?? Array.Empty<RoleInfo>(),
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
            var user = await _userRepository.GetByIdWithRolesAsync(id);
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
                Roles = user.UserRoles?.Select(ur => new RoleInfo { Id = ur.Role.Id, Code = ur.Role.Code, Name = ur.Role.Name }).ToArray() ?? Array.Empty<RoleInfo>()
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

            var user = await _userRepository.GetByIdWithRolesAsync(userBasic.Id);
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
                Roles = user.UserRoles?.Select(ur => new RoleInfo { Id = ur.Role.Id, Code = ur.Role.Code, Name = ur.Role.Name }).ToArray() ?? Array.Empty<RoleInfo>()
            };

            return Ok(response);
        }
    }
}
