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
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ISprintRepository _sprintRepository;
        private readonly IUserRepository _userRepository;
        private readonly IIncidentRepository _incidentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(
            IProjectRepository projectRepository,
            ISprintRepository sprintRepository,
            IUserRepository userRepository,
            IIncidentRepository incidentRepository,
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            ILogger<ProjectsController> logger)
        {
            _projectRepository = projectRepository;
            _sprintRepository = sprintRepository;
            _userRepository = userRepository;
            _incidentRepository = incidentRepository;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los proyectos
        /// </summary>
        /// <returns>Lista de proyectos</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Project>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            var projects = await _projectRepository.GetAllAsync();
            return Ok(projects);
        }

        /// <summary>
        /// Obtiene un proyecto por su ID
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <returns>Proyecto solicitado</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Project>> GetProject(Guid id)
        {
            var project = await _projectRepository.GetAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            return Ok(project);
        }

        /// <summary>
        /// Obtiene un proyecto por su código
        /// </summary>
        /// <param name="code">Código del proyecto</param>
        /// <returns>Proyecto solicitado</returns>
        [HttpGet("by-code/{code}")]
        [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Project>> GetProjectByCode(string code)
        {
            var project = await _projectRepository.GetByCodeAsync(code);
            if (project == null)
            {
                return NotFound();
            }
            return Ok(project);
        }

        /// <summary>
        /// Crea un nuevo proyecto (RF2.1)
        /// </summary>
        /// <param name="request">Datos del proyecto</param>
        /// <returns>Proyecto creado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Project), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Project>> CreateProject([FromBody] CreateProjectRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar si el código ya existe
            var existingProject = await _projectRepository.GetByCodeAsync(request.Code);
            if (existingProject != null)
            {
                return BadRequest(new { message = "A project with this code already exists" });
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                IsActive = true,
                CreatedBy = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _projectRepository.AddAsync(project);
            await _unitOfWork.SaveChangesAsync();

            // Auditoría
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            await _auditService.LogAsync(
                AuditAction.Create,
                userId,
                "Project",
                project.Id,
                ipAddress,
                userAgent,
                $"Created project: {project.Name}");

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }

        /// <summary>
        /// Actualiza un proyecto existente (RF2.1)
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <param name="request">Datos a actualizar</param>
        /// <returns>Proyecto actualizado</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Project>> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = await _projectRepository.GetAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            if (request.Name != null)
                project.Name = request.Name;
            if (request.Description != null)
                project.Description = request.Description;
            if (request.IsActive.HasValue)
                project.IsActive = request.IsActive.Value;

            project.UpdatedAt = DateTimeOffset.UtcNow;

            _projectRepository.Update(project);
            await _unitOfWork.SaveChangesAsync();

            // Auditoría
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            await _auditService.LogAsync(
                AuditAction.Update,
                userId,
                "Project",
                project.Id,
                ipAddress,
                userAgent,
                $"Updated project: {project.Name}");

            return Ok(project);
        }

        /// <summary>
        /// Elimina un proyecto (RF2.1)
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var project = await _projectRepository.GetAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _projectRepository.Remove(project);
            await _unitOfWork.SaveChangesAsync();

            // Auditoría
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            await _auditService.LogAsync(
                AuditAction.Delete,
                userId,
                "Project",
                id,
                ipAddress,
                userAgent,
                $"Deleted project: {project.Name}");

            return NoContent();
        }

        /// <summary>
        /// Obtiene los miembros de un proyecto (RF2.2)
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <returns>Lista de miembros</returns>
        [HttpGet("{id}/members")]
        [ProducesResponseType(typeof(IEnumerable<ProjectMember>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ProjectMember>>> GetProjectMembers(Guid id)
        {
            var project = await _projectRepository.GetAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var members = await _projectRepository.GetProjectMembersAsync(id);
            return Ok(members.ToArray());
        }

        /// <summary>
        /// Agrega un miembro al proyecto con un rol específico (RF2.2)
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <param name="request">Datos del miembro</param>
        /// <returns>Miembro agregado</returns>
        [HttpPost("{id}/members")]
        [ProducesResponseType(typeof(ProjectMember), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectMember>> AddProjectMember(Guid id, [FromBody] AddProjectMemberRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = await _projectRepository.GetAsync(id);
            if (project == null)
            {
                return NotFound(new { message = "Project not found" });
            }

            var user = await _userRepository.GetAsync(request.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            // Verificar si el usuario ya es miembro
            var existingMember = await _projectRepository.GetMemberAsync(id, request.UserId);
            if (existingMember != null)
            {
                return BadRequest(new { message = "User is already a member of this project" });
            }

            var member = new ProjectMember
            {
                ProjectId = id,
                UserId = request.UserId,
                RoleId = request.RoleId,
                JoinedAt = DateTimeOffset.UtcNow,
                IsActive = true
            };

            await _projectRepository.AddMemberAsync(member);

            // Auditoría
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            await _auditService.LogAsync(
                AuditAction.Assign,
                userId,
                "ProjectMember",
                id,
                ipAddress,
                userAgent,
                $"Added user {user.Username} to project {project.Name}");

            // Recargar el miembro con datos completos
            var createdMember = await _projectRepository.GetMemberAsync(id, request.UserId);
            return CreatedAtAction(nameof(GetProjectMembers), new { id }, createdMember);
        }

        /// <summary>
        /// Remueve un miembro del proyecto (RF2.2)
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <param name="userId">ID del usuario</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}/members/{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveProjectMember(Guid id, Guid userId)
        {
            var project = await _projectRepository.GetAsync(id);
            if (project == null)
            {
                return NotFound(new { message = "Project not found" });
            }

            var member = await _projectRepository.GetMemberAsync(id, userId);
            if (member == null)
            {
                return NotFound(new { message = "Member not found in this project" });
            }

            await _projectRepository.RemoveMemberAsync(id, userId);

            // Auditoría
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            await _auditService.LogAsync(
                AuditAction.Delete,
                currentUserId,
                "ProjectMember",
                id,
                ipAddress,
                userAgent,
                $"Removed user {member.User.Username} from project {project.Name}");

            return NoContent();
        }

        /// <summary>
        /// Obtiene el estado de avance de un proyecto (RF2.4)
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <returns>Estado de avance del proyecto</returns>
        [HttpGet("{id}/progress")]
        [ProducesResponseType(typeof(ProjectProgressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectProgressResponse>> GetProjectProgress(Guid id)
        {
            var project = await _projectRepository.GetAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            // Obtener sprints del proyecto
            var sprints = await _sprintRepository.GetByProjectIdAsync(id);
            var sprintsList = sprints.ToList();

            // Obtener incidencias del proyecto
            var incidents = await _incidentRepository.GetByProjectIdAsync(id);
            var incidentsList = incidents.ToList();

            // Obtener miembros del proyecto
            var members = await _projectRepository.GetProjectMembersAsync(id);
            var membersList = members.ToList();

            // Calcular estadísticas
            var totalSprints = sprintsList.Count;
            var activeSprints = sprintsList.Count(s => !s.IsClosed);
            var closedSprints = sprintsList.Count(s => s.IsClosed);

            var totalIncidents = incidentsList.Count;
            var openIncidents = incidentsList.Count(i => i.Status == IncidentStatus.Open);
            var inProgressIncidents = incidentsList.Count(i => i.Status == IncidentStatus.InProgress);
            var closedIncidents = incidentsList.Count(i => i.Status == IncidentStatus.Closed);

            var totalMembers = membersList.Count;
            var activeMembers = membersList.Count(m => m.IsActive);

            // Calcular porcentaje de completitud
            decimal completionPercentage = 0;
            if (totalIncidents > 0)
            {
                completionPercentage = Math.Round((decimal)closedIncidents / totalIncidents * 100, 2);
            }

            var progress = new ProjectProgressResponse
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                TotalSprints = totalSprints,
                ActiveSprints = activeSprints,
                ClosedSprints = closedSprints,
                TotalIncidents = totalIncidents,
                OpenIncidents = openIncidents,
                InProgressIncidents = inProgressIncidents,
                ClosedIncidents = closedIncidents,
                TotalMembers = totalMembers,
                ActiveMembers = activeMembers,
                CompletionPercentage = completionPercentage
            };

            return Ok(progress);
        }
    }
}
