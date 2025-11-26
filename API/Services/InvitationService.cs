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

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4a90d9; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #4a90d9; color: white; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>You've Been Invited!</h1>
        </div>
        <div class='content'>
            <p>Hello {invitation.FullName},</p>
            <p>You have been invited to join the BugMgr system as a <strong>{roleName}</strong>.</p>
            <p>Click the button below to complete your registration:</p>
            <p style='text-align: center;'>
                <a href='{invitationLink}' class='button'>Complete Registration</a>
            </p>
            <p>Or copy and paste this link into your browser:</p>
            <p style='word-break: break-all;'>{invitationLink}</p>
            <p><strong>This invitation will expire on {invitation.ExpiresAt:MMMM dd, yyyy 'at' HH:mm} UTC.</strong></p>
        </div>
        <div class='footer'>
            <p>This is an automated message from BugMgr System. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";

            var message = new EmailMessage
            {
                From = $"{_emailSettings.FromName} <{_emailSettings.FromEmail}>",
                To = [invitation.Email],
                Subject = "You've Been Invited to Join BugMgr",
                HtmlBody = htmlBody
            };

            await _resend.EmailSendAsync(message);
        }
    }
}
