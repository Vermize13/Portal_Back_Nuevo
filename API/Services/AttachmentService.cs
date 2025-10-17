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

        public AttachmentService(
            IAttachmentRepository attachmentRepository,
            IIncidentRepository incidentRepository,
            Repository.IUnitOfWork unitOfWork,
            IOptions<FileSettings> fileSettings,
            ILogger<AttachmentService> logger)
        {
            _attachmentRepository = attachmentRepository;
            _incidentRepository = incidentRepository;
            _unitOfWork = unitOfWork;
            _fileSettings = fileSettings.Value;
            _logger = logger;
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
            if (file.Length > _fileSettings.MaxFileSizeBytes)
            {
                throw new InvalidOperationException($"File size exceeds maximum allowed size of {_fileSettings.MaxFileSizeBytes} bytes");
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_fileSettings.AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"File extension {extension} is not allowed");
            }

            // Ensure storage directory exists
            var storageDir = Path.GetFullPath(_fileSettings.StoragePath);
            if (!Directory.Exists(storageDir))
            {
                Directory.CreateDirectory(storageDir);
            }

            // Create incident-specific directory
            var incidentDir = Path.Combine(storageDir, incidentId.ToString());
            if (!Directory.Exists(incidentDir))
            {
                Directory.CreateDirectory(incidentDir);
            }

            // Generate unique filename
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var uniqueFilename = $"{timestamp}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(incidentDir, uniqueFilename);

            // Save file and calculate checksum
            string checksum;
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
                fileStream.Position = 0;
                
                using var sha256 = SHA256.Create();
                var hashBytes = await sha256.ComputeHashAsync(fileStream);
                checksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
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
