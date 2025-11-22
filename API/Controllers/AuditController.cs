using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.DTOs;
using API.Services;

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

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        /// <summary>
        /// Get filtered audit logs with pagination
        /// Filter by user, action, and date range
        /// </summary>
        /// <param name="filter">Filter criteria including user ID, action, date range, and pagination</param>
        /// <returns>Paginated list of audit logs</returns>
        [HttpPost("logs")]
        public async Task<ActionResult<AuditLogPagedResponse>> GetAuditLogs([FromBody] AuditFilterRequest filter)
        {
            try
            {
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
        public async Task<IActionResult> ExportAuditLogs([FromBody] AuditFilterRequest filter)
        {
            try
            {
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
