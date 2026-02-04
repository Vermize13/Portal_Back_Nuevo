using Microsoft.AspNetCore.Mvc;
using Domain.Entity;
using Repository.Repositories;
using Repository;
using API.DTOs;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using BCrypt.Net;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;

        public PasswordController(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IAuditService auditService)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _auditService = auditService;
        }

        /// <summary>
        /// Request a password reset link
        /// </summary>
        [HttpPost("recover")]
        [AllowAnonymous]
        public async Task<IActionResult> RecoverPassword([FromBody] PasswordRecoveryRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest("Email is required");

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                // Return Ok even if user not found to prevent user enumeration
                return Ok(new { message = "If the email exists, a recovery link has been sent" });
            }

            // Generate token
            var token = Guid.NewGuid().ToString();
            user.ResetToken = token;
            user.ResetTokenExpires = DateTimeOffset.UtcNow.AddHours(24);

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            // Send email
            await _emailService.SendPasswordResetEmailAsync(user.Email, token);

            // Log action
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            await _auditService.LogAsync(AuditAction.Update, user.Id, "User", user.Id, ipAddress, userAgent, new { action = "PasswordRecoveryRequested" });

            return Ok(new { message = "If the email exists, a recovery link has been sent" });
        }

        /// <summary>
        /// Reset password using a token
        /// </summary>
        [HttpPost("reset")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetRequest request)
        {
            if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
                return BadRequest("Token and new password are required");

            // Find user by token manually (since repository doesn't have GetByToken)
            // Ideally should be in Repository but for now we iterate (not efficient but token is indexed usually or low volume)
            // Better: Add GetByResetToken to IUserRepository. But let's check if we can query.
            var allUsers = await _userRepository.GetAllAsync();
            var user = allUsers.FirstOrDefault(u => u.ResetToken == request.Token && u.ResetTokenExpires > DateTimeOffset.UtcNow);

            if (user == null)
            {
                return BadRequest(new { message = "Invalid or expired token" });
            }

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpires = null;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            // Log action
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            await _auditService.LogAsync(AuditAction.Update, user.Id, "User", user.Id, ipAddress, userAgent, new { action = "PasswordResetCompleted" });

            return Ok(new { message = "Password reset successfully" });
        }

        /// <summary>
        /// Change password for authenticated user
        /// </summary>
        [HttpPost("change")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeRequest request)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || Guid.Parse(userIdClaim) != request.UserId)
            {
                return Unauthorized();
            }

            var user = await _userRepository.GetByIdWithRoleAsync(request.UserId);
            if (user == null)
            {
                return NotFound();
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new { message = "Current password is incorrect" });
            }

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTimeOffset.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            // Log action
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            await _auditService.LogAsync(AuditAction.Update, user.Id, "User", user.Id, ipAddress, userAgent, new { action = "PasswordChanged" });

            return Ok(new { message = "Password changed successfully" });
        }
    }
}
