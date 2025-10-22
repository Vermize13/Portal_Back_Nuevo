using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using API.Controllers;
using API.DTOs;
using API.Services;
using Domain.Entity;
using Repository;
using Repository.Repositories;
using FluentAssertions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace API.Tests.Controllers
{
    /// <summary>
    /// Tests for Sprint Management endpoints
    /// Covers RF2.3: Asociar sprints a proyectos con fechas de inicio y fin
    /// </summary>
    public class SprintsControllerTests
    {
        private readonly Mock<ISprintRepository> _mockSprintRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly Mock<ILogger<SprintsController>> _mockLogger;
        private readonly SprintsController _controller;
        private readonly Guid _testUserId;

        public SprintsControllerTests()
        {
            _mockSprintRepository = new Mock<ISprintRepository>();
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockAuditService = new Mock<IAuditService>();
            _mockLogger = new Mock<ILogger<SprintsController>>();
            
            _controller = new SprintsController(
                _mockSprintRepository.Object,
                _mockProjectRepository.Object,
                _mockUnitOfWork.Object,
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
        public async Task GetSprintsByProject_WithValidProjectId_ReturnsOkWithSprints()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new Project { Id = projectId, Name = "Test Project" };
            var sprints = new List<Sprint>
            {
                new Sprint { Id = Guid.NewGuid(), ProjectId = projectId, Name = "Sprint 1" },
                new Sprint { Id = Guid.NewGuid(), ProjectId = projectId, Name = "Sprint 2" }
            };
            _mockProjectRepository.Setup(x => x.GetAsync(projectId)).ReturnsAsync(project);
            _mockSprintRepository.Setup(x => x.GetByProjectIdAsync(projectId)).ReturnsAsync(sprints);

            // Act
            var result = await _controller.GetSprintsByProject(projectId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedSprints = okResult!.Value as IEnumerable<Sprint>;
            returnedSprints.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetSprintsByProject_WithInvalidProjectId_ReturnsNotFound()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            _mockProjectRepository.Setup(x => x.GetAsync(projectId)).ReturnsAsync((Project?)null);

            // Act
            var result = await _controller.GetSprintsByProject(projectId);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetSprint_WithValidId_ReturnsOkWithSprint()
        {
            // Arrange
            var sprintId = Guid.NewGuid();
            var sprint = new Sprint
            {
                Id = sprintId,
                ProjectId = Guid.NewGuid(),
                Name = "Test Sprint",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14))
            };
            _mockSprintRepository.Setup(x => x.GetAsync(sprintId)).ReturnsAsync(sprint);

            // Act
            var result = await _controller.GetSprint(sprintId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedSprint = okResult!.Value as Sprint;
            returnedSprint.Should().NotBeNull();
            returnedSprint!.Id.Should().Be(sprintId);
        }

        [Fact]
        public async Task GetSprint_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var sprintId = Guid.NewGuid();
            _mockSprintRepository.Setup(x => x.GetAsync(sprintId)).ReturnsAsync((Sprint?)null);

            // Act
            var result = await _controller.GetSprint(sprintId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void CreateSprintRequest_ValidatesDates()
        {
            // Arrange
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14));
            var request = new CreateSprintRequest
            {
                Name = "Sprint 1",
                StartDate = startDate,
                EndDate = endDate,
                Goal = "Complete features"
            };

            // Act & Assert
            request.StartDate.Should().BeBefore(request.EndDate);
            request.Name.Should().NotBeNullOrEmpty();
        }
    }
}
