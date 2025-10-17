using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Services;

namespace WebApi.Controllers
{
    /// <summary>
    /// RF5 - Audit Management
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
        /// RF5.2: Get filtered audit logs with pagination
        /// Filter by user, action, and date range
        /// </summary>
        [HttpPost("logs")]
        public async Task<ActionResult<AuditLogPagedResponse>> GetAuditLogs([FromBody] AuditFilterRequest filter)
        {
            var logs = await _auditService.GetFilteredLogsAsync(filter);
            return Ok(logs);
        }

        /// <summary>
        /// RF5.3: Export audit logs to CSV format
        /// </summary>
        [HttpPost("export")]
        public async Task<IActionResult> ExportAuditLogs([FromBody] AuditFilterRequest filter)
        {
            var csvData = await _auditService.ExportLogsAsync(filter);
            
            var fileName = $"audit_logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            
            return File(csvData, "text/csv", fileName);
        }
    }
}
