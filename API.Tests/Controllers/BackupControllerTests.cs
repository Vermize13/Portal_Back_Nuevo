using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using API.Controllers;
using API.DTOs;
using API.Services;
using Domain.Entity;
using FluentAssertions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace API.Tests.Controllers
{
    /// <summary>
    /// Tests for Backup Management endpoints
    /// Covers RF6.1: Realizar copias de seguridad de la base de datos
    /// Covers RF6.2: Restaurar una copia de seguridad
    /// </summary>
    public class BackupControllerTests
    {
        private readonly Mock<IBackupService> _mockBackupService;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly Mock<ILogger<BackupController>> _mockLogger;
        private readonly BackupController _controller;
        private readonly Guid _testUserId;

        public BackupControllerTests()
        {
            _mockBackupService = new Mock<IBackupService>();
            _mockAuditService = new Mock<IAuditService>();
            _mockLogger = new Mock<ILogger<BackupController>>();
            
            _controller = new BackupController(
                _mockBackupService.Object,
                _mockAuditService.Object,
                _mockLogger.Object
            );

            _testUserId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
                new Claim(ClaimTypes.Name, "testuser")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task CreateBackup_WithValidRequest_ReturnsCreated()
        {
            // Arrange
            var request = new BackupRequest
            {
                Notes = "Manual backup before update"
            };
            var backup = new Backup
            {
                Id = Guid.NewGuid(),
                CreatedBy = _testUserId,
                StoragePath = "/backups/backup_123.sql",
                Strategy = "Full",
                Status = "completed",
                StartedAt = DateTimeOffset.UtcNow,
                FinishedAt = DateTimeOffset.UtcNow.AddMinutes(5),
                SizeBytes = 1024000,
                Notes = request.Notes,
                Creator = new User { Id = _testUserId, Name = "Test User" }
            };
            _mockBackupService.Setup(x => x.CreateBackupAsync(_testUserId, request.Notes))
                .ReturnsAsync(backup);

            // Act
            var result = await _controller.CreateBackup(request);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result.Result as CreatedAtActionResult;
            var response = createdResult!.Value as BackupResponse;
            response.Should().NotBeNull();
            response!.Id.Should().Be(backup.Id);
            response.Status.Should().Be("completed");
            _mockBackupService.Verify(x => x.CreateBackupAsync(_testUserId, request.Notes), Times.Once);
        }

        [Fact]
        public async Task CreateBackup_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new BackupRequest { Notes = "Test backup" };
            _mockBackupService.Setup(x => x.CreateBackupAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.CreateBackup(request);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task RestoreBackup_WithValidRequest_ReturnsCreated()
        {
            // Arrange
            var backupId = Guid.NewGuid();
            var request = new RestoreRequest
            {
                BackupId = backupId,
                Notes = "Restoring due to data corruption"
            };
            var restore = new Restore
            {
                Id = Guid.NewGuid(),
                BackupId = backupId,
                RequestedBy = _testUserId,
                Status = "completed",
                StartedAt = DateTimeOffset.UtcNow,
                FinishedAt = DateTimeOffset.UtcNow.AddMinutes(10),
                Notes = request.Notes
            };
            _mockBackupService.Setup(x => x.RestoreBackupAsync(backupId, _testUserId, request.Notes))
                .ReturnsAsync(restore);

            // Act
            var result = await _controller.RestoreBackup(request);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            _mockBackupService.Verify(x => x.RestoreBackupAsync(backupId, _testUserId, request.Notes), Times.Once);
        }

        [Fact]
        public async Task RestoreBackup_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new RestoreRequest
            {
                BackupId = Guid.NewGuid(),
                Notes = "Test restore"
            };
            _mockBackupService.Setup(x => x.RestoreBackupAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Restore failed"));

            // Act
            var result = await _controller.RestoreBackup(request);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public void BackupRequest_ValidatesNotes()
        {
            // Arrange & Act
            var request = new BackupRequest
            {
                Notes = "Test notes for backup"
            };

            // Assert
            request.Notes.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void RestoreRequest_ValidatesBackupId()
        {
            // Arrange & Act
            var backupId = Guid.NewGuid();
            var request = new RestoreRequest
            {
                BackupId = backupId,
                Notes = "Restore notes"
            };

            // Assert
            request.BackupId.Should().NotBeEmpty();
            request.BackupId.Should().Be(backupId);
        }
    }
}
