using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Services;
using API.DTOs;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BackupController : ControllerBase
    {
        private readonly IBackupService _backupService;
        private readonly IAuditService _auditService;
        private readonly ILogger<BackupController> _logger;

        public BackupController(
            IBackupService backupService,
            IAuditService auditService,
            ILogger<BackupController> logger)
        {
            _backupService = backupService;
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// RF6.1: Crea una copia de seguridad de la base de datos
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BackupResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BackupResponse>> CreateBackup([FromBody] BackupRequest request)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            try
            {
                var backup = await _backupService.CreateBackupAsync(userId, request.Notes);

                // Audit log
                await _auditService.LogAsync(
                    Domain.Entity.AuditAction.Create,
                    userId,
                    "Backup",
                    backup.Id,
                    GetClientIp(),
                    GetUserAgent(),
                    new { backup.Status, backup.StoragePath });

                var response = new BackupResponse
                {
                    Id = backup.Id,
                    CreatedBy = backup.CreatedBy,
                    CreatorName = backup.Creator?.Name ?? "Unknown",
                    StoragePath = backup.StoragePath,
                    Strategy = backup.Strategy,
                    SizeBytes = backup.SizeBytes,
                    Status = backup.Status,
                    StartedAt = backup.StartedAt,
                    FinishedAt = backup.FinishedAt,
                    Notes = backup.Notes
                };

                return CreatedAtAction(nameof(GetBackup), new { id = backup.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup");
                return StatusCode(500, "Error creating backup");
            }
        }

        /// <summary>
        /// RF6.2: Restaura una copia de seguridad de la base de datos
        /// </summary>
        [HttpPost("restore")]
        [ProducesResponseType(typeof(RestoreResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RestoreResponse>> RestoreBackup([FromBody] RestoreRequest request)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            try
            {
                var restore = await _backupService.RestoreBackupAsync(request.BackupId, userId, request.Notes);

                // Audit log
                await _auditService.LogAsync(
                    Domain.Entity.AuditAction.Update,
                    userId,
                    "Restore",
                    restore.Id,
                    GetClientIp(),
                    GetUserAgent(),
                    new { restore.Status, request.BackupId });

                var response = new RestoreResponse
                {
                    Id = restore.Id,
                    BackupId = restore.BackupId,
                    RequestedBy = restore.RequestedBy,
                    RequesterName = restore.Requester?.Name ?? "Unknown",
                    Status = restore.Status,
                    TargetDb = restore.TargetDb,
                    StartedAt = restore.StartedAt,
                    FinishedAt = restore.FinishedAt,
                    Notes = restore.Notes
                };

                return CreatedAtAction(nameof(GetRestore), new { id = restore.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring backup");
                return StatusCode(500, "Error restoring backup");
            }
        }

        /// <summary>
        /// Obtiene todas las copias de seguridad
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BackupResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BackupResponse>>> GetBackups()
        {
            var backups = await _backupService.GetBackupsAsync();
            var response = backups.Select(b => new BackupResponse
            {
                Id = b.Id,
                CreatedBy = b.CreatedBy,
                CreatorName = b.Creator?.Name ?? "Unknown",
                StoragePath = b.StoragePath,
                Strategy = b.Strategy,
                SizeBytes = b.SizeBytes,
                Status = b.Status,
                StartedAt = b.StartedAt,
                FinishedAt = b.FinishedAt,
                Notes = b.Notes
            });

            return Ok(response);
        }

        /// <summary>
        /// Obtiene una copia de seguridad por su ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BackupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BackupResponse>> GetBackup(Guid id)
        {
            var backup = await _backupService.GetBackupAsync(id);
            if (backup == null)
                return NotFound();

            var response = new BackupResponse
            {
                Id = backup.Id,
                CreatedBy = backup.CreatedBy,
                CreatorName = backup.Creator?.Name ?? "Unknown",
                StoragePath = backup.StoragePath,
                Strategy = backup.Strategy,
                SizeBytes = backup.SizeBytes,
                Status = backup.Status,
                StartedAt = backup.StartedAt,
                FinishedAt = backup.FinishedAt,
                Notes = backup.Notes
            };

            return Ok(response);
        }

        /// <summary>
        /// Obtiene información de una restauración por su ID
        /// </summary>
        [HttpGet("restore/{id}")]
        [ProducesResponseType(typeof(RestoreResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<ActionResult<RestoreResponse>> GetRestore(Guid id)
        {
            // This would need to be implemented in the service
            return Task.FromResult<ActionResult<RestoreResponse>>(NotFound("Restore lookup not implemented yet"));
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
