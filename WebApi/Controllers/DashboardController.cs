using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Services;

namespace WebApi.Controllers
{
    /// <summary>
    /// RF4 - Dynamic Dashboards
    /// Provides endpoints for dashboard metrics and analytics
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        /// <summary>
        /// RF4.1: Get incident metrics by status, priority, and severity
        /// </summary>
        [HttpPost("metrics")]
        public async Task<ActionResult<IncidentMetricsResponse>> GetIncidentMetrics([FromBody] DashboardFilterRequest filter)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var metrics = await _dashboardService.GetIncidentMetricsAsync(filter, userId);
            return Ok(metrics);
        }

        /// <summary>
        /// RF4.2: Get number of incidents opened and closed by sprint
        /// </summary>
        [HttpGet("sprints")]
        public async Task<ActionResult<List<SprintIncidentsResponse>>> GetSprintIncidents([FromQuery] Guid? projectId = null)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var sprints = await _dashboardService.GetSprintIncidentsAsync(projectId, userId);
            return Ok(sprints);
        }

        /// <summary>
        /// RF4.3: Calculate Mean Time To Resolution (MTTR)
        /// </summary>
        [HttpPost("mttr")]
        public async Task<ActionResult<MTTRResponse>> GetMTTR([FromBody] DashboardFilterRequest filter)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var mttr = await _dashboardService.GetMTTRAsync(filter, userId);
            return Ok(mttr);
        }

        /// <summary>
        /// RF4.4: Get incident evolution over time
        /// </summary>
        [HttpPost("evolution")]
        public async Task<ActionResult<List<IncidentEvolutionResponse>>> GetIncidentEvolution([FromBody] DashboardFilterRequest filter)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var evolution = await _dashboardService.GetIncidentEvolutionAsync(filter, userId);
            return Ok(evolution);
        }
    }
}
