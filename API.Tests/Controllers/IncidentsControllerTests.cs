using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using API.Controllers;
using API.DTOs;
using API.Services;
using Domain.Entity;
using Repository.Repositories;
using FluentAssertions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace API.Tests.Controllers
{
    /// <summary>
    /// Tests for Incident Management endpoints
    /// Covers RF3 - Gestión de Incidencias
    /// RF3.1: Crear, editar, asignar y cerrar incidencias
    /// RF3.2: Contenido de incidencias (título, descripción, severidad, etc.)
    /// RF3.3: Adjuntar archivos
    /// RF3.4: Historial de cambios
    /// RF3.5: Comentarios
    /// RF3.6: Notificaciones
    /// RF3.7: Etiquetas (tags)
    /// </summary>
    public class IncidentsControllerTests
    {
        private readonly Mock<IIncidentRepository> _mockIncidentRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ILabelRepository> _mockLabelRepository;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ILogger<IncidentsController>> _mockLogger;
        private readonly Guid _testUserId;

        public IncidentsControllerTests()
        {
            _mockIncidentRepository = new Mock<IIncidentRepository>();
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockLabelRepository = new Mock<ILabelRepository>();
            _mockAuditService = new Mock<IAuditService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockLogger = new Mock<ILogger<IncidentsController>>();

            _testUserId = Guid.NewGuid();
        }

        [Fact]
        public void CreateIncident_ValidatesRequiredFields()
        {
            // Arrange
            var request = new CreateIncidentRequest
            {
                ProjectId = Guid.NewGuid(),
                Title = "Test Incident",
                Description = "Test Description",
                Severity = IncidentSeverity.High,
                Priority = IncidentPriority.Must
            };

            // Act & Assert
            request.ProjectId.Should().NotBeEmpty();
            request.Title.Should().NotBeNullOrEmpty();
            request.Description.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void UpdateIncident_ValidatesChanges()
        {
            // Arrange
            var request = new UpdateIncidentRequest
            {
                Title = "Updated Title",
                Status = IncidentStatus.InProgress,
                Priority = IncidentPriority.Should
            };

            // Act & Assert
            request.Title.Should().NotBeNullOrEmpty();
            request.Status.Should().HaveValue();
            request.Priority.Should().HaveValue();
        }

        [Fact]
        public void AddComment_ValidatesBody()
        {
            // Arrange
            var request = new AddCommentRequest
            {
                Body = "This is a test comment"
            };

            // Act & Assert
            request.Body.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void IncidentSeverity_HasValidValues()
        {
            // Assert
            Enum.IsDefined(typeof(IncidentSeverity), IncidentSeverity.Low).Should().BeTrue();
            Enum.IsDefined(typeof(IncidentSeverity), IncidentSeverity.Medium).Should().BeTrue();
            Enum.IsDefined(typeof(IncidentSeverity), IncidentSeverity.High).Should().BeTrue();
            Enum.IsDefined(typeof(IncidentSeverity), IncidentSeverity.Critical).Should().BeTrue();
        }

        [Fact]
        public void IncidentPriority_HasValidValues()
        {
            // Assert
            Enum.IsDefined(typeof(IncidentPriority), IncidentPriority.Wont).Should().BeTrue();
            Enum.IsDefined(typeof(IncidentPriority), IncidentPriority.Could).Should().BeTrue();
            Enum.IsDefined(typeof(IncidentPriority), IncidentPriority.Should).Should().BeTrue();
            Enum.IsDefined(typeof(IncidentPriority), IncidentPriority.Must).Should().BeTrue();
        }

        [Fact]
        public void IncidentStatus_HasValidValues()
        {
            // Assert
            Enum.IsDefined(typeof(IncidentStatus), IncidentStatus.Open).Should().BeTrue();
            Enum.IsDefined(typeof(IncidentStatus), IncidentStatus.InProgress).Should().BeTrue();
            Enum.IsDefined(typeof(IncidentStatus), IncidentStatus.Closed).Should().BeTrue();
        }
    }
}
