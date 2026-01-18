using System.Security.Cryptography;
using API.DTOs;
using Domain.Entity;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Resend;

namespace API.Services
{
    public class InvitationService : IInvitationService
    {
        private readonly BugMgrDbContext _context;
        private readonly IResend _resend;
        private readonly EmailSettings _emailSettings;
        private readonly InvitationSettings _invitationSettings;
        private readonly IAuthService _authService;
        private readonly IAuditService _auditService;

        public InvitationService(
            BugMgrDbContext context,
            IResend resend,
            IOptions<EmailSettings> emailSettings,
            IOptions<InvitationSettings> invitationSettings,
            IAuthService authService,
            IAuditService auditService)
        {
            _context = context;
            _resend = resend;
            _emailSettings = emailSettings.Value;
            _invitationSettings = invitationSettings.Value;
            _authService = authService;
            _auditService = auditService;
        }

        public async Task<InvitationResponse?> InviteUserAsync(InviteUserRequest request, Guid invitedByUserId, string ipAddress, string userAgent)
        {
            // Check if email already exists as a user
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return null;
            }

            // Check if there's already a pending invitation for this email
            var existingInvitation = await _context.UserInvitations
                .FirstOrDefaultAsync(i => i.Email == request.Email && !i.IsUsed && i.ExpiresAt > DateTimeOffset.UtcNow);
            
            if (existingInvitation != null)
            {
                // Mark old invitation as used to allow new one
                existingInvitation.IsUsed = true;
            }

            // Verify role exists
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId);
            if (role == null)
            {
                return null;
            }

            // Generate secure token
            var token = GenerateSecureToken();

            var invitation = new UserInvitation
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FullName = request.FullName,
                RoleId = request.RoleId,
                Token = token,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(_invitationSettings.ExpirationHours),
                IsUsed = false,
                InvitedByUserId = invitedByUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.UserInvitations.Add(invitation);
            await _context.SaveChangesAsync();

            // Send invitation email
            await SendInvitationEmailAsync(invitation, role.Name);

            // Log the invitation action
            await _auditService.LogAsync(AuditAction.Create, invitedByUserId, "UserInvitation", invitation.Id, ipAddress, userAgent, new { email = request.Email, role = role.Name });

            return new InvitationResponse
            {
                Id = invitation.Id,
                Email = invitation.Email,
                FullName = invitation.FullName,
                RoleName = role.Name,
                ExpiresAt = invitation.ExpiresAt,
                Message = "Invitation sent successfully"
            };
        }

        public async Task<ValidateInvitationResponse> ValidateTokenAsync(string token)
        {
            var invitation = await _context.UserInvitations
                .Include(i => i.Role)
                .FirstOrDefaultAsync(i => i.Token == token);

            if (invitation == null)
            {
                return new ValidateInvitationResponse
                {
                    IsValid = false,
                    Message = "Invalid invitation token"
                };
            }

            if (invitation.IsUsed)
            {
                return new ValidateInvitationResponse
                {
                    IsValid = false,
                    Message = "This invitation has already been used"
                };
            }

            if (invitation.ExpiresAt < DateTimeOffset.UtcNow)
            {
                return new ValidateInvitationResponse
                {
                    IsValid = false,
                    Message = "This invitation has expired"
                };
            }

            return new ValidateInvitationResponse
            {
                IsValid = true,
                Email = invitation.Email,
                FullName = invitation.FullName,
                RoleName = invitation.Role?.Name,
                Message = "Invitation is valid"
            };
        }

        public async Task<AuthResponse?> CompleteInvitationAsync(CompleteInvitationRequest request, string ipAddress, string userAgent)
        {
            var invitation = await _context.UserInvitations
                .Include(i => i.Role)
                .FirstOrDefaultAsync(i => i.Token == request.Token);

            if (invitation == null || invitation.IsUsed || invitation.ExpiresAt < DateTimeOffset.UtcNow)
            {
                return null;
            }

            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return null;
            }

            // Check if email already exists (might have been registered in the meantime)
            if (await _context.Users.AnyAsync(u => u.Email == invitation.Email))
            {
                return null;
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = invitation.FullName,
                Email = invitation.Email,
                Username = request.Username,
                PasswordHash = passwordHash,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                RoleId = invitation.RoleId
            };

            _context.Users.Add(user);

            // Mark invitation as used
            invitation.IsUsed = true;
            invitation.UsedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            // Log the user creation
            await _auditService.LogAsync(AuditAction.Create, user.Id, "User", user.Id, ipAddress, userAgent, new { username = user.Username, fromInvitation = true });

            // Generate token
            var role = invitation.Role?.Code;
            var token = _authService.GenerateJwtToken(user.Id, user.Username, role);

            return new AuthResponse
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = role,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            };
        }

        private static string GenerateSecureToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        private async Task SendInvitationEmailAsync(UserInvitation invitation, string roleName)
        {
            var invitationLink = $"{_invitationSettings.FrontendBaseUrl}/register?token={Uri.EscapeDataString(invitation.Token)}";
            var logoUrl = $"{_invitationSettings.FrontendBaseUrl}/assets/martinierelogo.png";

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background: linear-gradient(135deg, #1e3a5f 0%, #2c5282 100%); color: white; padding: 30px 20px; text-align: center; }}
        .logo {{ max-width: 180px; margin-bottom: 15px; }}
        .header h1 {{ margin: 0; font-size: 24px; font-weight: 600; }}
        .content {{ padding: 30px 20px; background-color: #ffffff; }}
        .greeting {{ font-size: 18px; margin-bottom: 20px; }}
        .message {{ margin-bottom: 20px; color: #555; }}
        .role-badge {{ display: inline-block; background-color: #e8f4fd; color: #1e3a5f; padding: 4px 12px; border-radius: 4px; font-weight: 600; }}
        .button-container {{ text-align: center; margin: 30px 0; }}
        .button {{ display: inline-block; padding: 14px 32px; background: linear-gradient(135deg, #1e3a5f 0%, #2c5282 100%); color: white !important; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; }}
        .link-section {{ background-color: #f8f9fa; padding: 15px; border-radius: 6px; margin: 20px 0; }}
        .link-section p {{ margin: 5px 0; font-size: 13px; color: #666; }}
        .link-section a {{ color: #2c5282; word-break: break-all; }}
        .expiration {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 12px; border-radius: 6px; margin: 20px 0; }}
        .expiration p {{ margin: 0; color: #856404; font-size: 14px; }}
        .footer {{ text-align: center; padding: 20px; background-color: #f8f9fa; color: #666; font-size: 12px; border-top: 1px solid #eee; }}
        .footer p {{ margin: 5px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <img src='{logoUrl}' alt='Martiniere' class='logo' />
            <h1>¡Has sido invitado!</h1>
        </div>
        <div class='content'>
            <p class='greeting'>Hola <strong>{invitation.FullName}</strong>,</p>
            <p class='message'>Has sido invitado a unirte al Sistema de Gestión de Incidencias de Martiniere con el rol de <span class='role-badge'>{roleName}</span>.</p>
            <p class='message'>Para completar tu registro y activar tu cuenta, haz clic en el siguiente botón:</p>
            <div class='button-container'>
                <a href='{invitationLink}' class='button'>Completar Registro</a>
            </div>
            <div class='link-section'>
                <p>Si el botón no funciona, copia y pega este enlace en tu navegador:</p>
                <a href='{invitationLink}'>{invitationLink}</a>
            </div>
            <div class='expiration'>
                <p>⏰ <strong>Esta invitación expira el {invitation.ExpiresAt:dd 'de' MMMM 'de' yyyy 'a las' HH:mm} UTC.</strong></p>
            </div>
            <p class='message'>Si no solicitaste esta invitación, puedes ignorar este correo de forma segura.</p>
        </div>
        <div class='footer'>
            <p><strong>Martiniere - Sistema de Gestión de Incidencias</strong></p>
            <p>Este es un mensaje automático, por favor no respondas a este correo.</p>
        </div>
    </div>
</body>
</html>";

            var message = new EmailMessage
            {
                From = $"{_emailSettings.FromName} <{_emailSettings.FromEmail}>",
                To = [invitation.Email],
                Subject = "Invitación para unirte a Martiniere",
                HtmlBody = htmlBody
            };

            await _resend.EmailSendAsync(message);
        }
    }
}
