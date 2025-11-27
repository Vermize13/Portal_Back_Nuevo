using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.DTOs;
using API.Services;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvitationsController : ControllerBase
    {
        private readonly IInvitationService _invitationService;
        private readonly ILogger<InvitationsController> _logger;

        public InvitationsController(IInvitationService invitationService, ILogger<InvitationsController> logger)
        {
            _invitationService = invitationService;
            _logger = logger;
        }

        /// <summary>
        /// Invite a new user to the system. Only Admin, LiderTecnico, and ProductOwner roles can invite users.
        /// </summary>
        /// <param name="request">Invitation request containing full name, email, and role</param>
        /// <returns>Invitation response with details</returns>
        [HttpPost]
        [Authorize(Roles = "admin,lider_tecnico,product_owner")]
        [ProducesResponseType(typeof(InvitationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> InviteUser([FromBody] InviteUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user identity" });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            var response = await _invitationService.InviteUserAsync(request, userId, ipAddress, userAgent);

            if (response == null)
            {
                return BadRequest(new { message = "Unable to send invitation. The email may already be registered or the role may be invalid." });
            }

            return Ok(response);
        }

        /// <summary>
        /// Validate an invitation token
        /// </summary>
        /// <param name="token">The invitation token to validate</param>
        /// <returns>Validation result with invitation details if valid</returns>
        [HttpGet("validate/{token}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ValidateInvitationResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateToken(string token)
        {
            var response = await _invitationService.ValidateTokenAsync(token);
            return Ok(response);
        }

        /// <summary>
        /// Complete registration using an invitation token
        /// </summary>
        /// <param name="request">Registration details including token, username, and password</param>
        /// <returns>Authentication response with JWT token</returns>
        [HttpPost("complete")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CompleteInvitation([FromBody] CompleteInvitationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            var response = await _invitationService.CompleteInvitationAsync(request, ipAddress, userAgent);

            if (response == null)
            {
                return BadRequest(new { message = "Unable to complete registration. The invitation may be invalid, expired, or the username may already be taken." });
            }

            return Ok(response);
        }
    }
}
