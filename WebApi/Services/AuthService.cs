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
            // Find user with role
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !user.IsActive)
                return null;

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            var role = user.Role?.Code;
            var token = GenerateJwtToken(user.Id, user.Username, role);
            
            // Log the login action
            await _auditService.LogAsync(AuditAction.Login, user.Id, null, null, ipAddress, userAgent, new { username = user.Username });

            return new AuthResponse
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = role,
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

            // Get default role (Developer) if it exists
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "Developer");

            // Create user with single role
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Username = request.Username,
                PasswordHash = passwordHash,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                RoleId = defaultRole?.Id
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Log the create action
            await _auditService.LogAsync(AuditAction.Create, user.Id, "User", user.Id, ipAddress, userAgent, new { username = user.Username });

            var role = defaultRole?.Code;
            var token = GenerateJwtToken(user.Id, user.Username, role);

            return new AuthResponse
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = role,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
            };
        }

        public string GenerateJwtToken(Guid userId, string username, string? role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add role claim if user has a role
            if (!string.IsNullOrEmpty(role))
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
