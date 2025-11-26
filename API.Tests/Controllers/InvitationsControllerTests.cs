using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using API.Controllers;
using API.Services;
using API.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace API.Tests.Controllers
{
    /// <summary>
    /// Tests for User Invitation endpoints
    /// Covers user invitation via email with role-based authorization
    /// </summary>
    public class InvitationsControllerTests
    {
        private readonly Mock<IInvitationService> _mockInvitationService;
        private readonly Mock<ILogger<InvitationsController>> _mockLogger;
        private readonly InvitationsController _controller;

        public InvitationsControllerTests()
        {
            _mockInvitationService = new Mock<IInvitationService>();
            _mockLogger = new Mock<ILogger<InvitationsController>>();
            _controller = new InvitationsController(_mockInvitationService.Object, _mockLogger.Object);
            
            // Setup HttpContext with authenticated user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            _controller.HttpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        }

        [Fact]
        public async Task InviteUser_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new InviteUserRequest
            {
                FullName = "Test User",
                Email = "testuser@test.com",
                RoleId = Guid.NewGuid()
            };
            var expectedResponse = new InvitationResponse
            {
                Id = Guid.NewGuid(),
                Email = "testuser@test.com",
                FullName = "Test User",
                RoleName = "Developer",
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(48),
                Message = "Invitation sent successfully"
            };
            _mockInvitationService.Setup(x => x.InviteUserAsync(It.IsAny<InviteUserRequest>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.InviteUser(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task InviteUser_WhenEmailAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            var request = new InviteUserRequest
            {
                FullName = "Existing User",
                Email = "existing@test.com",
                RoleId = Guid.NewGuid()
            };
            _mockInvitationService.Setup(x => x.InviteUserAsync(It.IsAny<InviteUserRequest>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((InvitationResponse?)null);

            // Act
            var result = await _controller.InviteUser(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task InviteUser_WithoutAuthenticatedUser_ReturnsUnauthorized()
        {
            // Arrange
            var request = new InviteUserRequest
            {
                FullName = "Test User",
                Email = "testuser@test.com",
                RoleId = Guid.NewGuid()
            };
            
            // Setup controller without valid user claims
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };
            _controller.HttpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

            // Act
            var result = await _controller.InviteUser(request);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task ValidateToken_WithValidToken_ReturnsOkWithValidResponse()
        {
            // Arrange
            var token = "valid-token-123";
            var expectedResponse = new ValidateInvitationResponse
            {
                IsValid = true,
                Email = "testuser@test.com",
                FullName = "Test User",
                RoleName = "Developer",
                Message = "Invitation is valid"
            };
            _mockInvitationService.Setup(x => x.ValidateTokenAsync(token))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ValidateToken(token);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ValidateInvitationResponse;
            response!.IsValid.Should().BeTrue();
            response.Email.Should().Be("testuser@test.com");
        }

        [Fact]
        public async Task ValidateToken_WithInvalidToken_ReturnsOkWithInvalidResponse()
        {
            // Arrange
            var token = "invalid-token";
            var expectedResponse = new ValidateInvitationResponse
            {
                IsValid = false,
                Message = "Invalid invitation token"
            };
            _mockInvitationService.Setup(x => x.ValidateTokenAsync(token))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ValidateToken(token);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ValidateInvitationResponse;
            response!.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateToken_WithExpiredToken_ReturnsOkWithExpiredMessage()
        {
            // Arrange
            var token = "expired-token";
            var expectedResponse = new ValidateInvitationResponse
            {
                IsValid = false,
                Message = "This invitation has expired"
            };
            _mockInvitationService.Setup(x => x.ValidateTokenAsync(token))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ValidateToken(token);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ValidateInvitationResponse;
            response!.IsValid.Should().BeFalse();
            response.Message.Should().Contain("expired");
        }

        [Fact]
        public async Task CompleteInvitation_WithValidRequest_ReturnsOkWithAuthResponse()
        {
            // Arrange
            var request = new CompleteInvitationRequest
            {
                Token = "valid-token",
                Username = "newuser",
                Password = "password123"
            };
            var expectedResponse = new AuthResponse
            {
                Token = "jwt-token",
                Username = "newuser",
                Email = "newuser@test.com",
                Role = "Developer",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
            _mockInvitationService.Setup(x => x.CompleteInvitationAsync(It.IsAny<CompleteInvitationRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CompleteInvitation(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task CompleteInvitation_WithInvalidToken_ReturnsBadRequest()
        {
            // Arrange
            var request = new CompleteInvitationRequest
            {
                Token = "invalid-token",
                Username = "newuser",
                Password = "password123"
            };
            _mockInvitationService.Setup(x => x.CompleteInvitationAsync(It.IsAny<CompleteInvitationRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((AuthResponse?)null);

            // Act
            var result = await _controller.CompleteInvitation(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CompleteInvitation_WithExistingUsername_ReturnsBadRequest()
        {
            // Arrange
            var request = new CompleteInvitationRequest
            {
                Token = "valid-token",
                Username = "existinguser",
                Password = "password123"
            };
            _mockInvitationService.Setup(x => x.CompleteInvitationAsync(It.IsAny<CompleteInvitationRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((AuthResponse?)null);

            // Act
            var result = await _controller.CompleteInvitation(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
