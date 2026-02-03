using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.DTOs;
using API.Services;
using Infrastructure;
using System.Security.Claims;
using Repository.Repositories;

namespace API.Controllers
{
    /// <summary>
    /// Audit Management Controller
    /// Provides endpoints for audit log retrieval and export
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;
        private readonly Repository.Repositories.IUserRepository _userRepository; // Injected
        private readonly BugMgrDbContext _context; // Access to roles if needed, or via repository

        public AuditController(IAuditService auditService, Repository.Repositories.IUserRepository userRepository, BugMgrDbContext context)
        {
            _auditService = auditService;
            _userRepository = userRepository;
            _context = context;
        }

        /// <summary>
        /// Get filtered audit logs with pagination
        /// Filter by user, action, and date range
        /// </summary>
        /// <param name="filter">Filter criteria including user ID, action, date range, and pagination</param>
        /// <returns>Paginated list of audit logs</returns>
        [HttpPost("logs")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AuditLogPagedResponse>> GetAuditLogs([FromBody] AuditFilterRequest filter)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
                // Check if user has permission (Admin, Scrum Master, Product Owner)
                // Stakeholder, Tech Lead, Dev -> No Access
                
                var user = await _userRepository.GetByIdWithRoleAsync(userId);
                var roleCode = user?.Role?.Code?.ToLower() ?? "";
                
                var allowedRoles = new[] { "admin", "scrum_master", "product_owner" };
                
                if (!allowedRoles.Contains(roleCode))
                {
                    return Forbid();
                }

                var logs = await _auditService.GetFilteredLogsAsync(filter);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving audit logs", error = ex.Message });
            }
        }

        /// <summary>
        /// Export audit logs to CSV format
        /// </summary>
        /// <param name="filter">Filter criteria for logs to export</param>
        /// <returns>CSV file containing audit logs</returns>
        [HttpPost("export")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ExportAuditLogs([FromBody] AuditFilterRequest filter)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
                
                var user = await _userRepository.GetByIdWithRoleAsync(userId);
                var roleCode = user?.Role?.Code?.ToLower() ?? "";
                
                var allowedRoles = new[] { "admin", "scrum_master", "product_owner" };
                
                if (!allowedRoles.Contains(roleCode))
                {
                    return Forbid();
                }

                var csvData = await _auditService.ExportLogsAsync(filter);
                
                var fileName = $"audit_logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
                
                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while exporting audit logs", error = ex.Message });
            }
        }
    }
}
