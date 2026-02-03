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
    /// Tests for Project Management endpoints
    /// Covers RF2 - Gestión de Proyectos
    /// RF2.1: Crear, editar y eliminar proyectos
    /// RF2.2: Asignar miembros con distintos roles
    /// RF2.3: Asociar sprints a proyectos
    /// RF2.4: Mostrar el estado de avance
    /// </summary>
    public class ProjectsControllerTests
    {
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<ISprintRepository> _mockSprintRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IIncidentRepository> _mockIncidentRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly Mock<ILogger<ProjectsController>> _mockLogger;
        private readonly ProjectsController _controller;
        private readonly Guid _testUserId;

        public ProjectsControllerTests()
        {
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockSprintRepository = new Mock<ISprintRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockIncidentRepository = new Mock<IIncidentRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockAuditService = new Mock<IAuditService>();
            var mockRoleRepository = new Mock<IRoleRepository>();
            _mockLogger = new Mock<ILogger<ProjectsController>>();
            
            _controller = new ProjectsController(
                _mockProjectRepository.Object,
                _mockSprintRepository.Object,
                _mockUserRepository.Object,
                _mockIncidentRepository.Object,
                _mockUnitOfWork.Object,
                _mockAuditService.Object,
                mockRoleRepository.Object,
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
        public async Task GetProjects_ReturnsOkWithProjectList()
        {
            // Arrange
            var projects = new List<Project>
            {
                new Project { Id = Guid.NewGuid(), Name = "Project 1", Code = "PRJ1" },
                new Project { Id = Guid.NewGuid(), Name = "Project 2", Code = "PRJ2" }
            };
            _mockProjectRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(projects);

            // Act
            var result = await _controller.GetProjects();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedProjects = okResult!.Value as IEnumerable<Project>;
            returnedProjects.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetProject_WithValidId_ReturnsOkWithProject()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                Code = "TST",
                Description = "Test Description"
            };
            _mockProjectRepository.Setup(x => x.GetAsync(projectId)).ReturnsAsync(project);

            // Act
            var result = await _controller.GetProject(projectId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedProject = okResult!.Value as Project;
            returnedProject.Should().NotBeNull();
            returnedProject!.Id.Should().Be(projectId);
        }

        [Fact]
        public async Task GetProject_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            _mockProjectRepository.Setup(x => x.GetAsync(projectId)).ReturnsAsync((Project?)null);

            // Act
            var result = await _controller.GetProject(projectId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateProject_WithValidData_ReturnsCreated()
        {
            // Arrange
            var request = new CreateProjectRequest
            {
                Name = "New Project",
                Code = "NEW",
                Description = "New Project Description"
            };
            _mockProjectRepository.Setup(x => x.GetByCodeAsync(request.Code)).ReturnsAsync((Project?)null);
            _mockProjectRepository.Setup(x => x.AddAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.CreateProject(request);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            _mockProjectRepository.Verify(x => x.AddAsync(It.IsAny<Project>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateProject_WithExistingCode_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateProjectRequest
            {
                Name = "New Project",
                Code = "EXISTING",
                Description = "Description"
            };
            var existingProject = new Project { Id = Guid.NewGuid(), Code = "EXISTING" };
            _mockProjectRepository.Setup(x => x.GetByCodeAsync(request.Code)).ReturnsAsync(existingProject);

            // Act
            var result = await _controller.CreateProject(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateProject_WithValidData_ReturnsOk()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new Project
            {
                Id = projectId,
                Name = "Old Name",
                Description = "Old Description",
                IsActive = true
            };
            var request = new UpdateProjectRequest
            {
                Name = "Updated Name",
                Description = "Updated Description"
            };
            _mockProjectRepository.Setup(x => x.GetAsync(projectId)).ReturnsAsync(project);
            _mockProjectRepository.Setup(x => x.Update(It.IsAny<Project>()));
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateProject(projectId, request);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            _mockProjectRepository.Verify(x => x.Update(It.IsAny<Project>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteProject_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new Project { Id = projectId, Name = "Test Project" };
            _mockProjectRepository.Setup(x => x.GetAsync(projectId)).ReturnsAsync(project);
            _mockProjectRepository.Setup(x => x.Remove(project));
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.DeleteProject(projectId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockProjectRepository.Verify(x => x.Remove(It.IsAny<Project>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProjectMembers_WithValidProjectId_ReturnsOkWithMembers()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new Project { Id = projectId, Name = "Test Project" };
            var members = new List<ProjectMember>
            {
                new ProjectMember { ProjectId = projectId, UserId = Guid.NewGuid() },
                new ProjectMember { ProjectId = projectId, UserId = Guid.NewGuid() }
            };
            _mockProjectRepository.Setup(x => x.GetAsync(projectId)).ReturnsAsync(project);
            _mockProjectRepository.Setup(x => x.GetProjectMembersAsync(projectId)).ReturnsAsync(members);

            // Act
            var result = await _controller.GetProjectMembers(projectId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedMembers = okResult!.Value as IEnumerable<ProjectMember>;
            returnedMembers.Should().HaveCount(2);
        }

        [Fact]
        public async Task AddProjectMember_WithValidData_ReturnsCreated()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var project = new Project { Id = projectId, Name = "Test Project" };
            var user = new User { Id = userId, Username = "testuser" };
            var request = new AddProjectMemberRequest { UserId = userId, RoleId = roleId };
            
            _mockProjectRepository.Setup(x => x.GetAsync(projectId)).ReturnsAsync(project);
            _mockUserRepository.Setup(x => x.GetAsync(userId)).ReturnsAsync(user);
            _mockProjectRepository.Setup(x => x.GetMemberAsync(projectId, userId)).ReturnsAsync((ProjectMember?)null);
            _mockProjectRepository.Setup(x => x.AddMemberAsync(It.IsAny<ProjectMember>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddProjectMember(projectId, request);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            _mockProjectRepository.Verify(x => x.AddMemberAsync(It.IsAny<ProjectMember>()), Times.Once);
        }

        [Fact]
        public async Task RemoveProjectMember_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var project = new Project { Id = projectId, Name = "Test Project" };
            var member = new ProjectMember 
            { 
                ProjectId = projectId, 
                UserId = userId,
                User = new User { Id = userId, Username = "testuser" }
            };
            
            _mockProjectRepository.Setup(x => x.GetAsync(projectId)).ReturnsAsync(project);
            _mockProjectRepository.Setup(x => x.GetMemberAsync(projectId, userId)).ReturnsAsync(member);
            _mockProjectRepository.Setup(x => x.RemoveMemberAsync(projectId, userId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.RemoveProjectMember(projectId, userId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockProjectRepository.Verify(x => x.RemoveMemberAsync(projectId, userId), Times.Once);
        }

        [Fact]
        public async Task GetProjectProgress_WithValidProjectId_ReturnsOkWithProgress()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new Project { Id = projectId, Name = "Test Project" };
            var sprints = new List<Sprint>
            {
                new Sprint { Id = Guid.NewGuid(), ProjectId = projectId, IsClosed = false },
                new Sprint { Id = Guid.NewGuid(), ProjectId = projectId, IsClosed = true }
            };
            var incidents = new List<Incident>
            {
                new Incident { Id = Guid.NewGuid(), ProjectId = projectId, Status = IncidentStatus.Open },
                new Incident { Id = Guid.NewGuid(), ProjectId = projectId, Status = IncidentStatus.Closed }
            };
            var members = new List<ProjectMember>
            {
                new ProjectMember { ProjectId = projectId, UserId = Guid.NewGuid(), IsActive = true }
            };

            _mockProjectRepository.Setup(x => x.GetAsync(projectId)).ReturnsAsync(project);
            _mockSprintRepository.Setup(x => x.GetByProjectIdAsync(projectId)).ReturnsAsync(sprints);
            _mockIncidentRepository.Setup(x => x.GetByProjectIdAsync(projectId)).ReturnsAsync(incidents);
            _mockProjectRepository.Setup(x => x.GetProjectMembersAsync(projectId)).ReturnsAsync(members);

            // Act
            var result = await _controller.GetProjectProgress(projectId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var progress = okResult!.Value as ProjectProgressResponse;
            progress.Should().NotBeNull();
            progress!.TotalSprints.Should().Be(2);
            progress.TotalIncidents.Should().Be(2);
            progress.CompletionPercentage.Should().Be(50);
        }
    }
}
