using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Services;
using API.DTOs;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/incidents/{incidentId}/attachments")]
    [Authorize]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachmentService _attachmentService;
        private readonly IAuditService _auditService;
        private readonly ILogger<AttachmentsController> _logger;

        public AttachmentsController(
            IAttachmentService attachmentService,
            IAuditService auditService,
            ILogger<AttachmentsController> logger)
        {
            _attachmentService = attachmentService;
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// RF6.4 y RF6.5: Sube un archivo adjunto a una incidencia (con validación de tamaño)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(AttachmentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AttachmentResponse>> UploadAttachment(Guid incidentId, IFormFile file)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest("No file provided");

            try
            {
                var attachment = await _attachmentService.UploadAttachmentAsync(incidentId, userId, file);

                // Audit log
                await _auditService.LogAsync(
                    Domain.Entity.AuditAction.Create,
                    userId,
                    "Attachment",
                    attachment.Id,
                    GetClientIp(),
                    GetUserAgent(),
                    new { attachment.FileName, attachment.FileSizeBytes, incidentId });

                var response = new AttachmentResponse
                {
                    Id = attachment.Id,
                    IncidentId = attachment.IncidentId,
                    UploadedBy = attachment.UploadedBy,
                    UploaderName = attachment.Uploader?.Name ?? "Unknown",
                    FileName = attachment.FileName,
                    MimeType = attachment.MimeType,
                    FileSizeBytes = attachment.FileSizeBytes,
                    UploadedAt = attachment.UploadedAt
                };

                return CreatedAtAction(nameof(GetAttachment), new { incidentId, id = attachment.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachment");
                return StatusCode(500, "Error uploading attachment");
            }
        }

        /// <summary>
        /// RF6.4: Descarga un archivo adjunto
        /// </summary>
        [HttpGet("{id}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadAttachment(Guid incidentId, Guid id)
        {
            try
            {
                var (fileStream, fileName, contentType) = await _attachmentService.DownloadAttachmentAsync(id);

                // Audit log
                var userId = GetUserId();
                if (userId != Guid.Empty)
                {
                    await _auditService.LogAsync(
                        Domain.Entity.AuditAction.Download,
                        userId,
                        "Attachment",
                        id,
                        GetClientIp(),
                        GetUserAgent(),
                        new { fileName, incidentId });
                }

                return File(fileStream, contentType, fileName);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading attachment");
                return StatusCode(500, "Error downloading attachment");
            }
        }

        /// <summary>
        /// RF6.4: Obtiene todos los archivos adjuntos de una incidencia
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AttachmentResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AttachmentResponse>>> GetIncidentAttachments(Guid incidentId)
        {
            var attachments = await _attachmentService.GetIncidentAttachmentsAsync(incidentId);
            var response = attachments.Select(a => new AttachmentResponse
            {
                Id = a.Id,
                IncidentId = a.IncidentId,
                UploadedBy = a.UploadedBy,
                UploaderName = a.Uploader?.Name ?? "Unknown",
                FileName = a.FileName,
                MimeType = a.MimeType,
                FileSizeBytes = a.FileSizeBytes,
                UploadedAt = a.UploadedAt
            });

            return Ok(response);
        }

        /// <summary>
        /// RF6.4: Obtiene un archivo adjunto específico
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AttachmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AttachmentResponse>> GetAttachment(Guid incidentId, Guid id)
        {
            var attachment = await _attachmentService.GetAttachmentAsync(id);
            if (attachment == null || attachment.IncidentId != incidentId)
                return NotFound();

            var response = new AttachmentResponse
            {
                Id = attachment.Id,
                IncidentId = attachment.IncidentId,
                UploadedBy = attachment.UploadedBy,
                UploaderName = attachment.Uploader?.Name ?? "Unknown",
                FileName = attachment.FileName,
                MimeType = attachment.MimeType,
                FileSizeBytes = attachment.FileSizeBytes,
                UploadedAt = attachment.UploadedAt
            };

            return Ok(response);
        }

        /// <summary>
        /// RF6.4: Elimina un archivo adjunto
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAttachment(Guid incidentId, Guid id)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            try
            {
                var attachment = await _attachmentService.GetAttachmentAsync(id);
                if (attachment == null || attachment.IncidentId != incidentId)
                    return NotFound();

                await _attachmentService.DeleteAttachmentAsync(id);

                // Audit log
                await _auditService.LogAsync(
                    Domain.Entity.AuditAction.Delete,
                    userId,
                    "Attachment",
                    id,
                    GetClientIp(),
                    GetUserAgent(),
                    new { attachment.FileName, incidentId });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment");
                return StatusCode(500, "Error deleting attachment");
            }
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
