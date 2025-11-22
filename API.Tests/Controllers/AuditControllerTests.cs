using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using API.Controllers;
using API.Services;
using API.DTOs;
using FluentAssertions;
using Domain.Entity;

namespace API.Tests.Controllers
{
    /// <summary>
    /// Tests for Audit endpoints
    /// </summary>
    public class AuditControllerTests
    {
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly AuditController _controller;

        public AuditControllerTests()
        {
            _mockAuditService = new Mock<IAuditService>();
            _controller = new AuditController(_mockAuditService.Object);
        }

        [Fact]
        public async Task GetAuditLogs_WithValidFilter_ReturnsOkWithLogs()
        {
            // Arrange
            var filter = new AuditFilterRequest
            {
                Page = 1,
                PageSize = 10
            };

            var expectedResponse = new AuditLogPagedResponse
            {
                Logs = new List<AuditLogResponse>
                {
                    new AuditLogResponse
                    {
                        Id = Guid.NewGuid(),
                        Action = "HttpRequest",
                        ActorId = Guid.NewGuid(),
                        ActorUsername = "testuser",
                        HttpMethod = "GET",
                        HttpPath = "/api/test",
                        HttpStatusCode = 200,
                        DurationMs = 100,
                        CreatedAt = DateTimeOffset.UtcNow
                    }
                },
                TotalCount = 1,
                Page = 1,
                PageSize = 10,
                TotalPages = 1
            };

            _mockAuditService.Setup(x => x.GetFilteredLogsAsync(It.IsAny<AuditFilterRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAuditLogs(filter);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task GetAuditLogs_WithFilterByUser_ReturnsFilteredLogs()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var filter = new AuditFilterRequest
            {
                UserId = userId,
                Page = 1,
                PageSize = 10
            };

            var expectedResponse = new AuditLogPagedResponse
            {
                Logs = new List<AuditLogResponse>
                {
                    new AuditLogResponse
                    {
                        Id = Guid.NewGuid(),
                        Action = "Login",
                        ActorId = userId,
                        ActorUsername = "testuser",
                        CreatedAt = DateTimeOffset.UtcNow
                    }
                },
                TotalCount = 1,
                Page = 1,
                PageSize = 10,
                TotalPages = 1
            };

            _mockAuditService.Setup(x => x.GetFilteredLogsAsync(It.Is<AuditFilterRequest>(f => f.UserId == userId)))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAuditLogs(filter);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as AuditLogPagedResponse;
            response.Should().NotBeNull();
            response!.Logs.Should().HaveCount(1);
            response.Logs[0].ActorId.Should().Be(userId);
        }

        [Fact]
        public async Task GetAuditLogs_WithFilterByAction_ReturnsFilteredLogs()
        {
            // Arrange
            var filter = new AuditFilterRequest
            {
                Action = AuditAction.HttpRequest,
                Page = 1,
                PageSize = 10
            };

            var expectedResponse = new AuditLogPagedResponse
            {
                Logs = new List<AuditLogResponse>
                {
                    new AuditLogResponse
                    {
                        Id = Guid.NewGuid(),
                        Action = "HttpRequest",
                        HttpMethod = "POST",
                        HttpPath = "/api/users",
                        HttpStatusCode = 201,
                        CreatedAt = DateTimeOffset.UtcNow
                    }
                },
                TotalCount = 1,
                Page = 1,
                PageSize = 10,
                TotalPages = 1
            };

            _mockAuditService.Setup(x => x.GetFilteredLogsAsync(It.Is<AuditFilterRequest>(f => f.Action == AuditAction.HttpRequest)))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAuditLogs(filter);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as AuditLogPagedResponse;
            response.Should().NotBeNull();
            response!.Logs.Should().HaveCount(1);
            response.Logs[0].Action.Should().Be("HttpRequest");
        }

        [Fact]
        public async Task GetAuditLogs_WithDateRange_ReturnsFilteredLogs()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var filter = new AuditFilterRequest
            {
                StartDate = startDate,
                EndDate = endDate,
                Page = 1,
                PageSize = 10
            };

            var expectedResponse = new AuditLogPagedResponse
            {
                Logs = new List<AuditLogResponse>(),
                TotalCount = 0,
                Page = 1,
                PageSize = 10,
                TotalPages = 0
            };

            _mockAuditService.Setup(x => x.GetFilteredLogsAsync(It.IsAny<AuditFilterRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAuditLogs(filter);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ExportAuditLogs_WithValidFilter_ReturnsCsvFile()
        {
            // Arrange
            var filter = new AuditFilterRequest
            {
                Page = 1,
                PageSize = 10
            };

            var csvData = System.Text.Encoding.UTF8.GetBytes("Id,Action,ActorId\ntest-id,Login,user-id");
            
            _mockAuditService.Setup(x => x.ExportLogsAsync(It.IsAny<AuditFilterRequest>()))
                .ReturnsAsync(csvData);

            // Act
            var result = await _controller.ExportAuditLogs(filter);

            // Assert
            result.Should().BeOfType<FileContentResult>();
            var fileResult = result as FileContentResult;
            fileResult!.ContentType.Should().Be("text/csv");
            fileResult.FileDownloadName.Should().Contain("audit_logs_");
            fileResult.FileDownloadName.Should().EndWith(".csv");
            fileResult.FileContents.Should().BeEquivalentTo(csvData);
        }

        [Fact]
        public async Task ExportAuditLogs_WithEmptyResults_ReturnsEmptyCsvFile()
        {
            // Arrange
            var filter = new AuditFilterRequest();
            var csvData = System.Text.Encoding.UTF8.GetBytes("Id,Action,ActorId\n");
            
            _mockAuditService.Setup(x => x.ExportLogsAsync(It.IsAny<AuditFilterRequest>()))
                .ReturnsAsync(csvData);

            // Act
            var result = await _controller.ExportAuditLogs(filter);

            // Assert
            result.Should().BeOfType<FileContentResult>();
            var fileResult = result as FileContentResult;
            fileResult!.ContentType.Should().Be("text/csv");
        }
    }
}
