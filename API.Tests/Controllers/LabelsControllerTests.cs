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
    /// Tests for Labels Management endpoints
    /// Covers creating and retrieving project labels
    /// </summary>
    public class LabelsControllerTests
    {
        private readonly Mock<ILabelRepository> _mockLabelRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly Mock<ILogger<LabelsController>> _mockLogger;
        private readonly LabelsController _controller;
        private readonly Guid _testUserId;

        public LabelsControllerTests()
        {
            _mockLabelRepository = new Mock<ILabelRepository>();
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockAuditService = new Mock<IAuditService>();
            _mockLogger = new Mock<ILogger<LabelsController>>();

            _controller = new LabelsController(
                _mockLabelRepository.Object,
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
        public async Task CreateLabel_WithValidData_ReturnsCreatedLabel()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                Code = "TST",
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            var request = new CreateLabelRequest
            {
                ProjectId = projectId,
                Name = "Bug",
                ColorHex = "#FF0000"
            };

            _mockProjectRepository.Setup(x => x.GetAsync(projectId)).ReturnsAsync(project);
            _mockLabelRepository.Setup(x => x.AddAsync(It.IsAny<Label>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.CreateLabel(request);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result.Result as CreatedAtActionResult;
            var response = createdResult!.Value as LabelResponse;
            response.Should().NotBeNull();
            response!.Name.Should().Be("Bug");
            response.ColorHex.Should().Be("#FF0000");
            response.ProjectId.Should().Be(projectId);

            _mockLabelRepository.Verify(x => x.AddAsync(It.IsAny<Label>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateLabel_WithNonExistentProject_ReturnsNotFound()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = new CreateLabelRequest
            {
                ProjectId = projectId,
                Name = "Bug",
                ColorHex = "#FF0000"
            };

            _mockProjectRepository.Setup(x => x.GetAsync(projectId)).ReturnsAsync((Project?)null);

            // Act
            var result = await _controller.CreateLabel(request);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            _mockLabelRepository.Verify(x => x.AddAsync(It.IsAny<Label>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task GetProjectLabels_WithValidProjectId_ReturnsLabels()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                Code = "TST",
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            var labels = new List<Label>
            {
                new Label { Id = Guid.NewGuid(), ProjectId = projectId, Name = "Bug", ColorHex = "#FF0000" },
                new Label { Id = Guid.NewGuid(), ProjectId = projectId, Name = "Feature", ColorHex = "#00FF00" }
            };

            _mockProjectRepository.Setup(x => x.GetAsync(projectId)).ReturnsAsync(project);
            _mockLabelRepository.Setup(x => x.GetByProjectIdAsync(projectId)).ReturnsAsync(labels);

            // Act
            var result = await _controller.GetProjectLabels(projectId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedLabels = okResult!.Value as IEnumerable<LabelResponse>;
            returnedLabels.Should().NotBeNull();
            returnedLabels.Should().HaveCount(2);
            returnedLabels!.First().Name.Should().Be("Bug");
        }

        [Fact]
        public async Task GetProjectLabels_WithNonExistentProject_ReturnsNotFound()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            _mockProjectRepository.Setup(x => x.GetAsync(projectId)).ReturnsAsync((Project?)null);

            // Act
            var result = await _controller.GetProjectLabels(projectId);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            _mockLabelRepository.Verify(x => x.GetByProjectIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetProjectLabels_WithNoLabels_ReturnsEmptyList()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                Code = "TST",
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _mockProjectRepository.Setup(x => x.GetAsync(projectId)).ReturnsAsync(project);
            _mockLabelRepository.Setup(x => x.GetByProjectIdAsync(projectId)).ReturnsAsync(new List<Label>());

            // Act
            var result = await _controller.GetProjectLabels(projectId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedLabels = okResult!.Value as IEnumerable<LabelResponse>;
            returnedLabels.Should().NotBeNull();
            returnedLabels.Should().BeEmpty();
        }
    }
}
