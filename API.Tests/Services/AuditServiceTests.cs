using API.Services;
using Domain.Entity;
using FluentAssertions;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;
using Repository.Repositories;
using Xunit;

namespace API.Tests.Services
{
    public class AuditServiceTests : IDisposable
    {
        private readonly BugMgrDbContext _context;
        private readonly AuditService _auditService;
        private readonly Mock<IAuditLogRepository> _mockAuditLogRepository;

        public AuditServiceTests()
        {
            var options = new DbContextOptionsBuilder<BugMgrDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new BugMgrDbContext(options);
            _mockAuditLogRepository = new Mock<IAuditLogRepository>();
            _auditService = new AuditService(_context, _mockAuditLogRepository.Object);
        }

        [Fact]
        public async Task LogHttpRequestAsync_ShouldCreateAuditLog()
        {
            // Arrange
            var actorId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            // Act
            await _auditService.LogHttpRequestAsync(
                actorId,
                "POST",
                "/api/users",
                201,
                150,
                "192.168.1.1",
                "Mozilla/5.0",
                requestId);

            // Assert
            var auditLog = await _context.AuditLogs.FirstOrDefaultAsync(a => a.RequestId == requestId);
            auditLog.Should().NotBeNull();
            auditLog!.Action.Should().Be(AuditAction.HttpRequest);
            auditLog.ActorId.Should().Be(actorId);
            auditLog.HttpMethod.Should().Be("POST");
            auditLog.HttpPath.Should().Be("/api/users");
            auditLog.HttpStatusCode.Should().Be(201);
            auditLog.DurationMs.Should().Be(150);
            auditLog.IpAddress.Should().Be("192.168.1.1");
            auditLog.UserAgent.Should().Be("Mozilla/5.0");
        }

        [Fact]
        public async Task LogSqlCommandAsync_ShouldCreateAuditLog()
        {
            // Arrange
            var sqlCommand = "SELECT * FROM Users WHERE Id = @p0";
            var sqlParameters = "{\"@p0\":\"123e4567-e89b-12d3-a456-426614174000\"}";

            // Act
            await _auditService.LogSqlCommandAsync(sqlCommand, sqlParameters, 25);

            // Assert
            var auditLog = await _context.AuditLogs
                .FirstOrDefaultAsync(a => a.Action == AuditAction.SqlCommand);
            
            auditLog.Should().NotBeNull();
            auditLog!.SqlCommand.Should().Be(sqlCommand);
            auditLog.SqlParameters.Should().Be(sqlParameters);
            auditLog.DurationMs.Should().Be(25);
        }

        [Fact]
        public async Task LogAsync_ShouldCreateAuditLog_WithExistingAction()
        {
            // Arrange
            var actorId = Guid.NewGuid();
            var entityId = Guid.NewGuid();
            var details = new { OldValue = "A", NewValue = "B" };

            // Act
            await _auditService.LogAsync(
                AuditAction.Update,
                actorId,
                "User",
                entityId,
                "10.0.0.1",
                "TestAgent/1.0",
                details);

            // Assert
            var auditLog = await _context.AuditLogs
                .FirstOrDefaultAsync(a => a.Action == AuditAction.Update && a.EntityId == entityId);
            
            auditLog.Should().NotBeNull();
            auditLog!.ActorId.Should().Be(actorId);
            auditLog.EntityName.Should().Be("User");
            auditLog.DetailsJson.Should().Contain("OldValue");
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
