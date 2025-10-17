using Domain.Entity;
using Repository.Repositories;
using Microsoft.Extensions.Options;
using API.DTOs;
using System.Diagnostics;

namespace API.Services
{
    public class BackupService : IBackupService
    {
        private readonly IBackupRepository _backupRepository;
        private readonly IRestoreRepository _restoreRepository;
        private readonly Repository.IUnitOfWork _unitOfWork;
        private readonly BackupSettings _backupSettings;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BackupService> _logger;

        public BackupService(
            IBackupRepository backupRepository,
            IRestoreRepository restoreRepository,
            Repository.IUnitOfWork unitOfWork,
            IOptions<BackupSettings> backupSettings,
            IConfiguration configuration,
            ILogger<BackupService> logger)
        {
            _backupRepository = backupRepository;
            _restoreRepository = restoreRepository;
            _unitOfWork = unitOfWork;
            _backupSettings = backupSettings.Value;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Backup> CreateBackupAsync(Guid userId, string? notes = null)
        {
            try
            {
                // Ensure backup directory exists
                var backupDir = Path.GetFullPath(_backupSettings.StoragePath);
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                // Generate backup filename
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var filename = $"backup_{timestamp}.sql";
                var backupPath = Path.Combine(backupDir, filename);

                var backup = new Backup
                {
                    Id = Guid.NewGuid(),
                    CreatedBy = userId,
                    StoragePath = backupPath,
                    Strategy = "pg_dump",
                    Status = "running",
                    StartedAt = DateTimeOffset.UtcNow
                };

                await _backupRepository.AddAsync(backup);

                // Execute pg_dump
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var connParams = ParseConnectionString(connectionString ?? "");
                
                var pgDumpPath = Path.Combine(_backupSettings.PostgresPath, "pg_dump");
                var arguments = $"-h {connParams.Host} -p {connParams.Port} -U {connParams.Username} -d {connParams.Database} -F c -b -v -f \"{backupPath}\"";

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = pgDumpPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                // Set password as environment variable
                processStartInfo.Environment["PGPASSWORD"] = connParams.Password;

                using var process = Process.Start(processStartInfo);
                if (process == null)
                {
                    throw new InvalidOperationException("Failed to start pg_dump process");
                }

                await process.WaitForExitAsync();

                if (process.ExitCode == 0 && File.Exists(backupPath))
                {
                    var fileInfo = new FileInfo(backupPath);
                    backup.SizeBytes = fileInfo.Length;
                    backup.Status = "completed";
                    backup.FinishedAt = DateTimeOffset.UtcNow;
                    backup.Notes = notes;
                }
                else
                {
                    backup.Status = "failed";
                    backup.FinishedAt = DateTimeOffset.UtcNow;
                    backup.Notes = $"pg_dump failed with exit code {process.ExitCode}. {notes ?? ""}";
                }

                _backupRepository.Update(backup);
                await _unitOfWork.SaveChangesAsync();
                return backup;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup");
                throw;
            }
        }

        public async Task<Restore> RestoreBackupAsync(Guid backupId, Guid userId, string? notes = null)
        {
            try
            {
                var backup = await _backupRepository.GetAsync(backupId);
                if (backup == null)
                {
                    throw new InvalidOperationException("Backup not found");
                }

                if (!File.Exists(backup.StoragePath))
                {
                    throw new InvalidOperationException("Backup file not found");
                }

                var restore = new Restore
                {
                    Id = Guid.NewGuid(),
                    BackupId = backupId,
                    RequestedBy = userId,
                    Status = "running",
                    StartedAt = DateTimeOffset.UtcNow,
                    Notes = notes
                };

                await _restoreRepository.AddAsync(restore);

                // Execute pg_restore
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var connParams = ParseConnectionString(connectionString ?? "");
                
                restore.TargetDb = connParams.Database;

                var pgRestorePath = Path.Combine(_backupSettings.PostgresPath, "pg_restore");
                var arguments = $"-h {connParams.Host} -p {connParams.Port} -U {connParams.Username} -d {connParams.Database} -c -v \"{backup.StoragePath}\"";

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = pgRestorePath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                // Set password as environment variable
                processStartInfo.Environment["PGPASSWORD"] = connParams.Password;

                using var process = Process.Start(processStartInfo);
                if (process == null)
                {
                    throw new InvalidOperationException("Failed to start pg_restore process");
                }

                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    restore.Status = "completed";
                    restore.FinishedAt = DateTimeOffset.UtcNow;
                }
                else
                {
                    restore.Status = "failed";
                    restore.FinishedAt = DateTimeOffset.UtcNow;
                    restore.Notes = $"pg_restore failed with exit code {process.ExitCode}. {notes ?? ""}";
                }

                _restoreRepository.Update(restore);
                await _unitOfWork.SaveChangesAsync();
                return restore;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring backup");
                throw;
            }
        }

        public async Task<IEnumerable<Backup>> GetBackupsAsync()
        {
            return await _backupRepository.GetAllAsync();
        }

        public async Task<Backup?> GetBackupAsync(Guid id)
        {
            return await _backupRepository.GetAsync(id);
        }

        private (string Host, string Port, string Database, string Username, string Password) ParseConnectionString(string connectionString)
        {
            var parts = connectionString.Split(';');
            var host = "localhost";
            var port = "5432";
            var database = "";
            var username = "";
            var password = "";

            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0].Trim().ToLower();
                    var value = keyValue[1].Trim();

                    switch (key)
                    {
                        case "host":
                            host = value;
                            break;
                        case "port":
                            port = value;
                            break;
                        case "database":
                            database = value;
                            break;
                        case "username":
                            username = value;
                            break;
                        case "password":
                            password = value;
                            break;
                    }
                }
            }

            return (host, port, database, username, password);
        }
    }
}
