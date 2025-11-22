using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Domain.Entity;
using Repository;
using Repository.Repositories;
using API.DTOs;
using API.Services;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SprintsController : ControllerBase
    {
        private readonly ISprintRepository _sprintRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly ILogger<SprintsController> _logger;

        public SprintsController(
            ISprintRepository sprintRepository,
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            ILogger<SprintsController> logger)
        {
            _sprintRepository = sprintRepository;
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los sprints de un proyecto (RF2.3)
        /// </summary>
        /// <param name="projectId">ID del proyecto</param>
        /// <returns>Lista de sprints</returns>
        [HttpGet("by-project/{projectId}")]
        [ProducesResponseType(typeof(IEnumerable<Sprint>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Sprint>>> GetSprintsByProject(Guid projectId)
        {
            var project = await _projectRepository.GetAsync(projectId);
            if (project == null)
            {
                return NotFound(new { message = "Project not found" });
            }

            var sprints = await _sprintRepository.GetByProjectIdAsync(projectId);
            return Ok(sprints.ToArray());
        }

        /// <summary>
        /// Obtiene un sprint por su ID
        /// </summary>
        /// <param name="id">ID del sprint</param>
        /// <returns>Sprint solicitado</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Sprint), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Sprint>> GetSprint(Guid id)
        {
            var sprint = await _sprintRepository.GetAsync(id);
            if (sprint == null)
            {
                return NotFound();
            }
            return Ok(sprint);
        }

        /// <summary>
        /// Crea un nuevo sprint asociado a un proyecto (RF2.3)
        /// </summary>
        /// <param name="projectId">ID del proyecto</param>
        /// <param name="request">Datos del sprint</param>
        /// <returns>Sprint creado</returns>
        [HttpPost("by-project/{projectId}")]
        [ProducesResponseType(typeof(Sprint), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Sprint>> CreateSprint(Guid projectId, [FromBody] CreateSprintRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = await _projectRepository.GetAsync(projectId);
            if (project == null)
            {
                return NotFound(new { message = "Project not found" });
            }

            if (request.EndDate < request.StartDate)
            {
                return BadRequest(new { message = "End date must be after start date" });
            }

            var sprint = new Sprint
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = request.Name,
                Goal = request.Goal,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsClosed = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _sprintRepository.AddAsync(sprint);
            await _unitOfWork.SaveChangesAsync();

            // Auditoría
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            await _auditService.LogAsync(
                AuditAction.Create,
                userId,
                "Sprint",
                sprint.Id,
                ipAddress,
                userAgent,
                $"Created sprint: {sprint.Name} for project {project.Name}");

            return CreatedAtAction(nameof(GetSprint), new { id = sprint.Id }, sprint);
        }

        /// <summary>
        /// Cierra un sprint
        /// </summary>
        /// <param name="id">ID del sprint</param>
        /// <returns>Sprint actualizado</returns>
        [HttpPatch("{id}/close")]
        [ProducesResponseType(typeof(Sprint), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Sprint>> CloseSprint(Guid id)
        {
            var sprint = await _sprintRepository.GetAsync(id);
            if (sprint == null)
            {
                return NotFound();
            }

            sprint.IsClosed = true;
            _sprintRepository.Update(sprint);
            await _unitOfWork.SaveChangesAsync();

            // Auditoría
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            await _auditService.LogAsync(
                AuditAction.Update,
                userId,
                "Sprint",
                sprint.Id,
                ipAddress,
                userAgent,
                $"Closed sprint: {sprint.Name}");

            return Ok(sprint);
        }

        /// <summary>
        /// Elimina un sprint
        /// </summary>
        /// <param name="id">ID del sprint</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSprint(Guid id)
        {
            var sprint = await _sprintRepository.GetAsync(id);
            if (sprint == null)
            {
                return NotFound();
            }

            _sprintRepository.Remove(sprint);
            await _unitOfWork.SaveChangesAsync();

            // Auditoría
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            await _auditService.LogAsync(
                AuditAction.Delete,
                userId,
                "Sprint",
                id,
                ipAddress,
                userAgent,
                $"Deleted sprint: {sprint.Name}");

            return NoContent();
        }
    }
}
