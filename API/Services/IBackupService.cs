using Domain.Entity;

namespace API.Services
{
    public interface IBackupService
    {
        Task<Backup> CreateBackupAsync(Guid userId, string? notes = null);
        Task<Restore> RestoreBackupAsync(Guid backupId, Guid userId, string? notes = null);
        Task<IEnumerable<Backup>> GetBackupsAsync();
        Task<Backup?> GetBackupAsync(Guid id);
    }
}
