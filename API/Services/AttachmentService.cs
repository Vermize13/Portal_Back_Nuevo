using Domain.Entity;
using Repository.Repositories;
using Microsoft.Extensions.Options;
using API.DTOs;
using System.Security.Cryptography;

namespace API.Services
{
    public class AttachmentService : IAttachmentService
    {
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IIncidentRepository _incidentRepository;
        private readonly Repository.IUnitOfWork _unitOfWork;
        private readonly FileSettings _fileSettings;
        private readonly ILogger<AttachmentService> _logger;
        private readonly IIncidentHistoryService _incidentHistoryService;
        private readonly ISystemConfigurationService _configService;

        public AttachmentService(
            IAttachmentRepository attachmentRepository,
            IIncidentRepository incidentRepository,
            Repository.IUnitOfWork unitOfWork,
            IOptions<FileSettings> fileSettings,
            ILogger<AttachmentService> logger,
            IIncidentHistoryService incidentHistoryService,
            ISystemConfigurationService configService)
        {
            _attachmentRepository = attachmentRepository;
            _incidentRepository = incidentRepository;
            _unitOfWork = unitOfWork;
            _fileSettings = fileSettings.Value;
            _logger = logger;
            _incidentHistoryService = incidentHistoryService;
            _configService = configService;
        }

        public async Task<Attachment> UploadAttachmentAsync(Guid incidentId, Guid userId, IFormFile file)
        {
            // Validate incident exists
            var incident = await _incidentRepository.GetAsync(incidentId);
            if (incident == null)
            {
                throw new InvalidOperationException("Incident not found");
            }

            // Validate file size
            var maxSizeMB = await _configService.GetIntAsync("MaxUploadSize", 10); // Default 10MB
            var maxSizeBytes = maxSizeMB * 1024 * 1024;
            
            if (file.Length > maxSizeBytes)
            {
                throw new InvalidOperationException($"File size exceeds maximum allowed size of {maxSizeMB} MB");
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_fileSettings.AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"File extension {extension} is not allowed");
            }

            string filePath;
            string checksum;

            try 
            {
                // Ensure storage directory exists
                var storageDir = Path.GetFullPath(_fileSettings.StoragePath);
                if (!Directory.Exists(storageDir))
                {
                    _logger.LogInformation("Creating storage directory: {StoragePath}", storageDir);
                    Directory.CreateDirectory(storageDir);
                }

                // Create incident-specific directory
                var incidentDir = Path.Combine(storageDir, incidentId.ToString());
                if (!Directory.Exists(incidentDir))
                {
                    _logger.LogInformation("Creating incident directory: {IncidentDir}", incidentDir);
                    Directory.CreateDirectory(incidentDir);
                }

                // Generate unique filename
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var uniqueFilename = $"{timestamp}_{Guid.NewGuid()}{extension}";
                filePath = Path.Combine(incidentDir, uniqueFilename);

                // Save file and calculate checksum
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                    fileStream.Position = 0;
                    
                    using var sha256 = SHA256.Create();
                    var hashBytes = await sha256.ComputeHashAsync(fileStream);
                    checksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save attachment file for incident {IncidentId}", incidentId);
                throw new InvalidOperationException($"Failed to save file: {ex.Message}", ex);
            }

            // Create attachment record
            var attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                IncidentId = incidentId,
                UploadedBy = userId,
                FileName = file.FileName,
                StoragePath = filePath,
                MimeType = file.ContentType,
                FileSizeBytes = file.Length,
                Sha256Checksum = checksum,
                UploadedAt = DateTimeOffset.UtcNow
            };

            await _attachmentRepository.AddAsync(attachment);
            _logger.LogInformation("Attachment {FileName} uploaded for incident {IncidentId}", file.FileName, incidentId);

            // Log incident history
            await _incidentHistoryService.LogAsync(incidentId, userId, "Attachment", null, attachment.FileName);

            return attachment;
        }

        public async Task<(Stream FileStream, string FileName, string ContentType)> DownloadAttachmentAsync(Guid attachmentId)
        {
            var attachment = await _attachmentRepository.GetAsync(attachmentId);
            if (attachment == null)
            {
                throw new InvalidOperationException("Attachment not found");
            }

            if (!File.Exists(attachment.StoragePath))
            {
                throw new InvalidOperationException("Attachment file not found on disk");
            }

            var fileStream = new FileStream(attachment.StoragePath, FileMode.Open, FileAccess.Read);
            return (fileStream, attachment.FileName, attachment.MimeType);
        }

        public async Task<IEnumerable<Attachment>> GetIncidentAttachmentsAsync(Guid incidentId)
        {
            return await _attachmentRepository.GetByIncidentIdAsync(incidentId);
        }

        public async Task DeleteAttachmentAsync(Guid attachmentId)
        {
            var attachment = await _attachmentRepository.GetAsync(attachmentId);
            if (attachment == null)
            {
                throw new InvalidOperationException("Attachment not found");
            }

            // Delete file from disk
            if (File.Exists(attachment.StoragePath))
            {
                File.Delete(attachment.StoragePath);
                _logger.LogInformation("Deleted attachment file {FilePath}", attachment.StoragePath);
            }

            // Log history before removing
            await _incidentHistoryService.LogAsync(attachment.IncidentId, attachment.UploadedBy, "Attachment", attachment.FileName, null);

            // Delete record from database
            _attachmentRepository.Remove(attachment);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Deleted attachment record {AttachmentId}", attachmentId);
        }

        public async Task<Attachment?> GetAttachmentAsync(Guid id)
        {
            return await _attachmentRepository.GetAsync(id);
        }
    }
}
