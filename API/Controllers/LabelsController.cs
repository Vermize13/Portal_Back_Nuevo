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
    public class LabelsController : ControllerBase
    {
        private readonly ILabelRepository _labelRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly ILogger<LabelsController> _logger;

        public LabelsController(
            ILabelRepository labelRepository,
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            ILogger<LabelsController> logger)
        {
            _labelRepository = labelRepository;
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Crea una nueva etiqueta para un proyecto
        /// </summary>
        /// <param name="request">Datos de la etiqueta</param>
        /// <returns>Etiqueta creada</returns>
        [HttpPost]
        [ProducesResponseType(typeof(LabelResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LabelResponse>> CreateLabel([FromBody] CreateLabelRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar que el proyecto existe
            var project = await _projectRepository.GetAsync(request.ProjectId);
            if (project == null)
            {
                return NotFound(new { message = "Project not found" });
            }

            var label = new Label
            {
                Id = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                Name = request.Name,
                ColorHex = request.ColorHex
            };

            await _labelRepository.AddAsync(label);
            await _unitOfWork.SaveChangesAsync();

            // Auditoría
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            await _auditService.LogAsync(
                AuditAction.Create,
                userId,
                "Label",
                label.Id,
                ipAddress,
                userAgent,
                $"Created label '{label.Name}' for project {project.Name}");

            var response = new LabelResponse
            {
                Id = label.Id,
                ProjectId = label.ProjectId,
                Name = label.Name,
                ColorHex = label.ColorHex
            };

            return CreatedAtAction(nameof(GetProjectLabels), new { projectId = label.ProjectId }, response);
        }

        /// <summary>
        /// Obtiene todas las etiquetas de un proyecto
        /// </summary>
        /// <param name="projectId">ID del proyecto</param>
        /// <returns>Lista de etiquetas del proyecto</returns>
        [HttpGet("project/{projectId}")]
        [ProducesResponseType(typeof(IEnumerable<LabelResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<LabelResponse>>> GetProjectLabels(Guid projectId)
        {
            // Verificar que el proyecto existe
            var project = await _projectRepository.GetAsync(projectId);
            if (project == null)
            {
                return NotFound(new { message = "Project not found" });
            }

            var labels = await _labelRepository.GetByProjectIdAsync(projectId);
            
            var response = labels.Select(l => new LabelResponse
            {
                Id = l.Id,
                ProjectId = l.ProjectId,
                Name = l.Name,
                ColorHex = l.ColorHex
            });

            return Ok(response);
        }
    }
}
