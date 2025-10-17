using Domain.Entity;

namespace API.Services
{
    public interface IAttachmentService
    {
        Task<Attachment> UploadAttachmentAsync(Guid incidentId, Guid userId, IFormFile file);
        Task<(Stream FileStream, string FileName, string ContentType)> DownloadAttachmentAsync(Guid attachmentId);
        Task<IEnumerable<Attachment>> GetIncidentAttachmentsAsync(Guid incidentId);
        Task DeleteAttachmentAsync(Guid attachmentId);
        Task<Attachment?> GetAttachmentAsync(Guid id);
    }
}
