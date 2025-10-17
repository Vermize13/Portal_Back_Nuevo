using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Domain.Entity;
using Repository.Repositories;
using API.DTOs;
using API.Services;
using Infrastructure;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IncidentsController : ControllerBase
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly BugMgrDbContext _context;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<IncidentsController> _logger;

        public IncidentsController(
            IIncidentRepository incidentRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            ILabelRepository labelRepository,
            BugMgrDbContext context,
            IAuditService auditService,
            INotificationService notificationService,
            ILogger<IncidentsController> logger)
        {
            _incidentRepository = incidentRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _labelRepository = labelRepository;
            _context = context;
            _auditService = auditService;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las incidencias
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<IncidentResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<IncidentResponse>>> GetIncidents(
            [FromQuery] Guid? projectId = null,
            [FromQuery] Guid? sprintId = null,
            [FromQuery] IncidentStatus? status = null,
            [FromQuery] IncidentSeverity? severity = null,
            [FromQuery] IncidentPriority? priority = null,
            [FromQuery] Guid? assigneeId = null,
            [FromQuery] Guid? reporterId = null)
        {
            var query = _context.Incidents
                .Include(i => i.Reporter)
                .Include(i => i.Assignee)
                .Include(i => i.Labels).ThenInclude(il => il.Label)
                .Include(i => i.Comments)
                .AsQueryable();

            // Note: Attachments are loaded separately for performance

            if (projectId.HasValue)
                query = query.Where(i => i.ProjectId == projectId.Value);

            if (sprintId.HasValue)
                query = query.Where(i => i.SprintId == sprintId.Value);

            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);

            if (severity.HasValue)
                query = query.Where(i => i.Severity == severity.Value);

            if (priority.HasValue)
                query = query.Where(i => i.Priority == priority.Value);

            if (assigneeId.HasValue)
                query = query.Where(i => i.AssigneeId == assigneeId.Value);

            if (reporterId.HasValue)
                query = query.Where(i => i.ReporterId == reporterId.Value);

            var incidents = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();

            var response = incidents.Select(i => MapToResponse(i)).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Obtiene una incidencia por su ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IncidentResponse>> GetIncident(Guid id)
        {
            var incident = await _context.Incidents
                .Include(i => i.Reporter)
                .Include(i => i.Assignee)
                .Include(i => i.Labels).ThenInclude(il => il.Label)
                .Include(i => i.Comments)
                .FirstOrDefaultAsync(i => i.Id == id);

            // Note: Attachments count is loaded separately for performance

            if (incident == null)
                return NotFound();

            return Ok(MapToResponse(incident));
        }

        /// <summary>
        /// Crea una nueva incidencia
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IncidentResponse>> CreateIncident([FromBody] CreateIncidentRequest request)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            // Validate project exists
            var project = await _projectRepository.GetAsync(request.ProjectId);
            if (project == null)
                return BadRequest("Project not found");

            // Validate sprint if provided
            if (request.SprintId.HasValue)
            {
                var sprint = await _context.Sprints.FindAsync(request.SprintId.Value);
                if (sprint == null || sprint.ProjectId != request.ProjectId)
                    return BadRequest("Sprint not found or does not belong to the project");
            }

            // Validate assignee if provided
            if (request.AssigneeId.HasValue)
            {
                var assignee = await _userRepository.GetAsync(request.AssigneeId.Value);
                if (assignee == null)
                    return BadRequest("Assignee not found");
            }

            // Generate incident code
            var incidentCount = await _context.Incidents.CountAsync(i => i.ProjectId == request.ProjectId);
            var code = $"{project.Code}-{incidentCount + 1}";

            var incident = new Incident
            {
                Id = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                SprintId = request.SprintId,
                Code = code,
                Title = request.Title,
                Description = request.Description,
                Severity = request.Severity,
                Priority = request.Priority,
                Status = IncidentStatus.Open,
                ReporterId = userId,
                AssigneeId = request.AssigneeId,
                StoryPoints = request.StoryPoints,
                DueDate = request.DueDate,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _incidentRepository.AddAsync(incident);

            // Add labels if provided
            if (request.LabelIds != null && request.LabelIds.Any())
            {
                foreach (var labelId in request.LabelIds)
                {
                    var label = await _labelRepository.GetAsync(labelId);
                    if (label != null && label.ProjectId == request.ProjectId)
                    {
                        _context.IncidentLabels.Add(new IncidentLabel
                        {
                            IncidentId = incident.Id,
                            LabelId = labelId
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            // Send notification if assigned
            if (request.AssigneeId.HasValue)
            {
                await _notificationService.NotifyIncidentAssignmentAsync(incident, request.AssigneeId.Value);
            }

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Create,
                userId,
                "Incident",
                incident.Id,
                GetClientIp(),
                GetUserAgent(),
                new { incident.Code, incident.Title, incident.Status });

            // Reload incident with relations
            var createdIncident = await _context.Incidents
                .Include(i => i.Reporter)
                .Include(i => i.Assignee)
                .Include(i => i.Labels).ThenInclude(il => il.Label)
                .FirstOrDefaultAsync(i => i.Id == incident.Id);

            return CreatedAtAction(nameof(GetIncident), new { id = incident.Id }, MapToResponse(createdIncident!));
        }

        /// <summary>
        /// Actualiza una incidencia existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IncidentResponse>> UpdateIncident(Guid id, [FromBody] UpdateIncidentRequest request)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var incident = await _context.Incidents
                .Include(i => i.Reporter)
                .Include(i => i.Assignee)
                .Include(i => i.Labels).ThenInclude(il => il.Label)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (incident == null)
                return NotFound();

            var changes = new List<string>();
            var oldAssigneeId = incident.AssigneeId;
            var oldStatus = incident.Status;

            // Track changes and update fields
            if (request.Title != null && request.Title != incident.Title)
            {
                await TrackChange(id, userId, "Title", incident.Title, request.Title);
                incident.Title = request.Title;
                changes.Add("Title");
            }

            if (request.Description != null && request.Description != incident.Description)
            {
                await TrackChange(id, userId, "Description", incident.Description, request.Description);
                incident.Description = request.Description;
                changes.Add("Description");
            }

            if (request.Severity.HasValue && request.Severity != incident.Severity)
            {
                await TrackChange(id, userId, "Severity", incident.Severity.ToString(), request.Severity.ToString());
                incident.Severity = request.Severity.Value;
                changes.Add("Severity");
            }

            if (request.Priority.HasValue && request.Priority != incident.Priority)
            {
                await TrackChange(id, userId, "Priority", incident.Priority.ToString(), request.Priority.ToString());
                incident.Priority = request.Priority.Value;
                changes.Add("Priority");
            }

            if (request.Status.HasValue && request.Status != incident.Status)
            {
                await TrackChange(id, userId, "Status", incident.Status.ToString(), request.Status.ToString());
                incident.Status = request.Status.Value;
                changes.Add("Status");

                // Set ClosedAt if status is Closed
                if (request.Status == IncidentStatus.Closed && incident.ClosedAt == null)
                {
                    incident.ClosedAt = DateTimeOffset.UtcNow;
                }
            }

            if (request.SprintId != incident.SprintId)
            {
                await TrackChange(id, userId, "SprintId", incident.SprintId?.ToString(), request.SprintId?.ToString());
                incident.SprintId = request.SprintId;
                changes.Add("SprintId");
            }

            if (request.AssigneeId != incident.AssigneeId)
            {
                await TrackChange(id, userId, "AssigneeId", incident.AssigneeId?.ToString(), request.AssigneeId?.ToString());
                incident.AssigneeId = request.AssigneeId;
                changes.Add("AssigneeId");
            }

            if (request.StoryPoints != incident.StoryPoints)
            {
                await TrackChange(id, userId, "StoryPoints", incident.StoryPoints?.ToString(), request.StoryPoints?.ToString());
                incident.StoryPoints = request.StoryPoints;
                changes.Add("StoryPoints");
            }

            if (request.DueDate != incident.DueDate)
            {
                await TrackChange(id, userId, "DueDate", incident.DueDate?.ToString(), request.DueDate?.ToString());
                incident.DueDate = request.DueDate;
                changes.Add("DueDate");
            }

            if (changes.Any())
            {
                incident.UpdatedAt = DateTimeOffset.UtcNow;
                await _context.SaveChangesAsync();

                // Send notifications
                if (request.AssigneeId.HasValue && request.AssigneeId != oldAssigneeId)
                {
                    await _notificationService.NotifyIncidentAssignmentAsync(incident, request.AssigneeId.Value);
                }

                if (request.Status.HasValue && request.Status != oldStatus)
                {
                    await _notificationService.NotifyIncidentStatusChangeAsync(incident, oldStatus, request.Status.Value);
                }

                // Audit log
                await _auditService.LogAsync(
                    AuditAction.Update,
                    userId,
                    "Incident",
                    incident.Id,
                    GetClientIp(),
                    GetUserAgent(),
                    new { changes, incident.Code });
            }

            return Ok(MapToResponse(incident));
        }

        /// <summary>
        /// Asigna una incidencia a un usuario
        /// </summary>
        [HttpPost("{id}/assign/{assigneeId}")]
        [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IncidentResponse>> AssignIncident(Guid id, Guid assigneeId)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var incident = await _context.Incidents
                .Include(i => i.Reporter)
                .Include(i => i.Assignee)
                .Include(i => i.Labels).ThenInclude(il => il.Label)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (incident == null)
                return NotFound("Incident not found");

            var assignee = await _userRepository.GetAsync(assigneeId);
            if (assignee == null)
                return NotFound("Assignee not found");

            var oldAssigneeId = incident.AssigneeId;
            incident.AssigneeId = assigneeId;
            incident.UpdatedAt = DateTimeOffset.UtcNow;

            await TrackChange(id, userId, "AssigneeId", oldAssigneeId?.ToString(), assigneeId.ToString());
            await _context.SaveChangesAsync();

            // Send notification
            await _notificationService.NotifyIncidentAssignmentAsync(incident, assigneeId);

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Assign,
                userId,
                "Incident",
                incident.Id,
                GetClientIp(),
                GetUserAgent(),
                new { incident.Code, assigneeId });

            return Ok(MapToResponse(incident));
        }

        /// <summary>
        /// Cierra una incidencia
        /// </summary>
        [HttpPost("{id}/close")]
        [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IncidentResponse>> CloseIncident(Guid id)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var incident = await _context.Incidents
                .Include(i => i.Reporter)
                .Include(i => i.Assignee)
                .Include(i => i.Labels).ThenInclude(il => il.Label)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (incident == null)
                return NotFound();

            var oldStatus = incident.Status;
            incident.Status = IncidentStatus.Closed;
            incident.ClosedAt = DateTimeOffset.UtcNow;
            incident.UpdatedAt = DateTimeOffset.UtcNow;

            await TrackChange(id, userId, "Status", oldStatus.ToString(), IncidentStatus.Closed.ToString());
            await _context.SaveChangesAsync();

            // Send notification
            await _notificationService.NotifyIncidentStatusChangeAsync(incident, oldStatus, IncidentStatus.Closed);

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Transition,
                userId,
                "Incident",
                incident.Id,
                GetClientIp(),
                GetUserAgent(),
                new { incident.Code, oldStatus, newStatus = IncidentStatus.Closed });

            return Ok(MapToResponse(incident));
        }

        /// <summary>
        /// Obtiene el historial de cambios de una incidencia
        /// </summary>
        [HttpGet("{id}/history")]
        [ProducesResponseType(typeof(IEnumerable<IncidentHistory>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<IncidentHistory>>> GetIncidentHistory(Guid id)
        {
            var incident = await _incidentRepository.GetAsync(id);
            if (incident == null)
                return NotFound();

            var history = await _context.IncidentHistories
                .Include(h => h.ChangedByUser)
                .Where(h => h.IncidentId == id)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();

            return Ok(history);
        }

        /// <summary>
        /// Añade un comentario a una incidencia
        /// </summary>
        [HttpPost("{id}/comments")]
        [ProducesResponseType(typeof(IncidentComment), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IncidentComment>> AddComment(Guid id, [FromBody] AddCommentRequest request)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var incident = await _incidentRepository.GetAsync(id);
            if (incident == null)
                return NotFound();

            var comment = new IncidentComment
            {
                Id = Guid.NewGuid(),
                IncidentId = id,
                AuthorId = userId,
                Body = request.Body,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.IncidentComments.Add(comment);
            await _context.SaveChangesAsync();

            // Reload with author
            var createdComment = await _context.IncidentComments
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Create,
                userId,
                "IncidentComment",
                comment.Id,
                GetClientIp(),
                GetUserAgent(),
                new { incidentId = id, incident.Code });

            return CreatedAtAction(nameof(GetIncidentComments), new { id }, createdComment);
        }

        /// <summary>
        /// Obtiene los comentarios de una incidencia
        /// </summary>
        [HttpGet("{id}/comments")]
        [ProducesResponseType(typeof(IEnumerable<IncidentComment>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<IncidentComment>>> GetIncidentComments(Guid id)
        {
            var incident = await _incidentRepository.GetAsync(id);
            if (incident == null)
                return NotFound();

            var comments = await _context.IncidentComments
                .Include(c => c.Author)
                .Where(c => c.IncidentId == id)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return Ok(comments);
        }

        /// <summary>
        /// Añade etiquetas a una incidencia
        /// </summary>
        [HttpPost("{id}/labels/{labelId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddLabel(Guid id, Guid labelId)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var incident = await _incidentRepository.GetAsync(id);
            if (incident == null)
                return NotFound("Incident not found");

            var label = await _labelRepository.GetAsync(labelId);
            if (label == null)
                return NotFound("Label not found");

            if (label.ProjectId != incident.ProjectId)
                return BadRequest("Label does not belong to the incident's project");

            var exists = await _context.IncidentLabels
                .AnyAsync(il => il.IncidentId == id && il.LabelId == labelId);

            if (!exists)
            {
                _context.IncidentLabels.Add(new IncidentLabel
                {
                    IncidentId = id,
                    LabelId = labelId
                });
                await _context.SaveChangesAsync();

                // Audit log
                await _auditService.LogAsync(
                    AuditAction.Update,
                    userId,
                    "Incident",
                    incident.Id,
                    GetClientIp(),
                    GetUserAgent(),
                    new { action = "AddLabel", incident.Code, labelId, label.Name });
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina una etiqueta de una incidencia
        /// </summary>
        [HttpDelete("{id}/labels/{labelId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveLabel(Guid id, Guid labelId)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var incident = await _incidentRepository.GetAsync(id);
            if (incident == null)
                return NotFound("Incident not found");

            var incidentLabel = await _context.IncidentLabels
                .FirstOrDefaultAsync(il => il.IncidentId == id && il.LabelId == labelId);

            if (incidentLabel != null)
            {
                _context.IncidentLabels.Remove(incidentLabel);
                await _context.SaveChangesAsync();

                // Audit log
                await _auditService.LogAsync(
                    AuditAction.Update,
                    userId,
                    "Incident",
                    incident.Id,
                    GetClientIp(),
                    GetUserAgent(),
                    new { action = "RemoveLabel", incident.Code, labelId });
            }

            return NoContent();
        }

        // Helper methods
        private IncidentResponse MapToResponse(Incident incident)
        {
            // Count attachments for this incident
            var attachmentCount = _context.Attachments.Count(a => a.IncidentId == incident.Id);

            return new IncidentResponse
            {
                Id = incident.Id,
                ProjectId = incident.ProjectId,
                SprintId = incident.SprintId,
                Code = incident.Code,
                Title = incident.Title,
                Description = incident.Description,
                Severity = incident.Severity,
                Priority = incident.Priority,
                Status = incident.Status,
                ReporterId = incident.ReporterId,
                ReporterName = incident.Reporter?.Name ?? "Unknown",
                AssigneeId = incident.AssigneeId,
                AssigneeName = incident.Assignee?.Name,
                StoryPoints = incident.StoryPoints,
                DueDate = incident.DueDate,
                CreatedAt = incident.CreatedAt,
                UpdatedAt = incident.UpdatedAt,
                ClosedAt = incident.ClosedAt,
                Labels = incident.Labels?.Select(il => new LabelInfo
                {
                    Id = il.LabelId,
                    Name = il.Label?.Name ?? "",
                    ColorHex = il.Label?.ColorHex
                }).ToList() ?? new List<LabelInfo>(),
                CommentCount = incident.Comments?.Count ?? 0,
                AttachmentCount = attachmentCount
            };
        }

        private Task TrackChange(Guid incidentId, Guid userId, string fieldName, string? oldValue, string? newValue)
        {
            var history = new IncidentHistory
            {
                Id = Guid.NewGuid(),
                IncidentId = incidentId,
                ChangedBy = userId,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                ChangedAt = DateTimeOffset.UtcNow
            };

            _context.IncidentHistories.Add(history);
            return Task.CompletedTask;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        private string? GetClientIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        private string? GetUserAgent()
        {
            return HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
        }
    }
}
