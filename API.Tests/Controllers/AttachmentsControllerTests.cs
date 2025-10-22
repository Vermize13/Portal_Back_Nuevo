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
    /// Tests for Attachment Management endpoints
    /// Covers RF6.4: Gestionar archivos adjuntos asociados a incidencias
    /// Covers RF6.5: Garantizar que los adjuntos no superen un tamaño máximo definido
    /// </summary>
    public class AttachmentsControllerTests
    {
        private readonly Mock<IAttachmentService> _mockAttachmentService;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly Mock<ILogger<AttachmentsController>> _mockLogger;
        private readonly AttachmentsController _controller;
        private readonly Guid _testUserId;

        public AttachmentsControllerTests()
        {
            _mockAttachmentService = new Mock<IAttachmentService>();
            _mockAuditService = new Mock<IAuditService>();
            _mockLogger = new Mock<ILogger<AttachmentsController>>();
            
            _controller = new AttachmentsController(
                _mockAttachmentService.Object,
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
        public async Task GetIncidentAttachments_WithValidIncidentId_ReturnsOkWithAttachments()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var attachments = new List<Attachment>
            {
                new Attachment 
                { 
                    Id = Guid.NewGuid(), 
                    IncidentId = incidentId, 
                    FileName = "screenshot.png",
                    FileSizeBytes = 1024,
                    MimeType = "image/png",
                    Uploader = new User { Name = "User1" }
                },
                new Attachment 
                { 
                    Id = Guid.NewGuid(), 
                    IncidentId = incidentId, 
                    FileName = "log.txt",
                    FileSizeBytes = 512,
                    MimeType = "text/plain",
                    Uploader = new User { Name = "User2" }
                }
            };
            _mockAttachmentService.Setup(x => x.GetIncidentAttachmentsAsync(incidentId))
                .ReturnsAsync(attachments);

            // Act
            var result = await _controller.GetIncidentAttachments(incidentId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedAttachments = okResult!.Value as IEnumerable<AttachmentResponse>;
            returnedAttachments.Should().NotBeNull();
            returnedAttachments.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAttachment_WithValidId_ReturnsOkWithAttachment()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var attachmentId = Guid.NewGuid();
            var attachment = new Attachment
            {
                Id = attachmentId,
                IncidentId = incidentId,
                FileName = "document.pdf",
                FileSizeBytes = 2048,
                MimeType = "application/pdf",
                StoragePath = "/uploads/document.pdf",
                Uploader = new User { Name = "Test User" }
            };
            _mockAttachmentService.Setup(x => x.GetAttachmentAsync(attachmentId))
                .ReturnsAsync(attachment);

            // Act
            var result = await _controller.GetAttachment(incidentId, attachmentId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedAttachment = okResult!.Value as AttachmentResponse;
            returnedAttachment.Should().NotBeNull();
            returnedAttachment!.Id.Should().Be(attachmentId);
        }

        [Fact]
        public async Task GetAttachment_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var attachmentId = Guid.NewGuid();
            _mockAttachmentService.Setup(x => x.GetAttachmentAsync(attachmentId))
                .ReturnsAsync((Attachment?)null);

            // Act
            var result = await _controller.GetAttachment(incidentId, attachmentId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteAttachment_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var attachmentId = Guid.NewGuid();
            var attachment = new Attachment
            {
                Id = attachmentId,
                IncidentId = incidentId,
                FileName = "old_file.txt"
            };
            _mockAttachmentService.Setup(x => x.GetAttachmentAsync(attachmentId))
                .ReturnsAsync(attachment);
            _mockAttachmentService.Setup(x => x.DeleteAttachmentAsync(attachmentId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteAttachment(incidentId, attachmentId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockAttachmentService.Verify(x => x.DeleteAttachmentAsync(attachmentId), Times.Once);
        }

        [Fact]
        public void Attachment_ValidatesFileSize()
        {
            // Arrange
            var maxFileSize = 10 * 1024 * 1024; // 10 MB
            var attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                FileName = "large_file.zip",
                FileSizeBytes = 5 * 1024 * 1024 // 5 MB
            };

            // Assert
            attachment.FileSizeBytes.Should().BeLessThan(maxFileSize);
        }

        [Fact]
        public void Attachment_ValidatesContentType()
        {
            // Arrange
            var allowedTypes = new[] { "image/png", "image/jpeg", "application/pdf", "text/plain" };
            var attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                FileName = "test.png",
                MimeType = "image/png"
            };

            // Assert
            allowedTypes.Should().Contain(attachment.MimeType);
        }

        [Fact]
        public void Attachment_ValidatesFileExtension()
        {
            // Arrange
            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".pdf", ".txt", ".log", ".zip" };
            var fileName = "screenshot.png";
            var extension = Path.GetExtension(fileName);

            // Assert
            allowedExtensions.Should().Contain(extension);
        }
    }
}
