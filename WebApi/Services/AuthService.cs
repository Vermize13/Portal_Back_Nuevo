using Domain.Entity;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApi.DTOs;

namespace WebApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly BugMgrDbContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly IAuditService _auditService;

        public AuthService(BugMgrDbContext context, IOptions<JwtSettings> jwtSettings, IAuditService auditService)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
            _auditService = auditService;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request, string ipAddress, string userAgent)
        {
            // Find user with roles
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !user.IsActive)
                return null;

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            var roles = user.UserRoles.Select(ur => ur.Role.Code).ToList();
            var token = GenerateJwtToken(user.Id, user.Username, roles);
            
            // Log the login action
            await _auditService.LogAsync(AuditAction.Login, user.Id, null, null, ipAddress, userAgent, new { username = user.Username });

            return new AuthResponse
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Roles = roles,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
            };
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, string ipAddress, string userAgent)
        {
            // Check if username or email already exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
                return null;

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Username = request.Username,
                PasswordHash = passwordHash,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Users.Add(user);

            // Assign default role (Developer) if it exists
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "Developer");
            if (defaultRole != null)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = defaultRole.Id,
                    AssignedAt = DateTimeOffset.UtcNow
                };
                _context.UserRoles.Add(userRole);
            }

            await _context.SaveChangesAsync();

            // Log the create action
            await _auditService.LogAsync(AuditAction.Create, user.Id, "User", user.Id, ipAddress, userAgent, new { username = user.Username });

            var roles = defaultRole != null ? new List<string> { defaultRole.Code } : new List<string>();
            var token = GenerateJwtToken(user.Id, user.Username, roles);

            return new AuthResponse
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Roles = roles,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
            };
        }

        public string GenerateJwtToken(Guid userId, string username, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
