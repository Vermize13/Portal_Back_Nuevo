using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using API.Controllers;
using API.Services;
using API.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace API.Tests.Controllers
{
    /// <summary>
    /// Tests for Authentication endpoints
    /// Covers RNF1.1 (JWT Authentication)
    /// </summary>
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
            
            // Setup HttpContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.HttpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "testpassword"
            };
            var expectedResponse = new AuthResponse
            {
                Token = "valid-jwt-token",
                Username = "testuser",
                Email = "testuser@test.com",
                Role = "User",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
            _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "wrongpassword"
            };
            _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((AuthResponse?)null);

            // Act
            var result = await _controller.Login(request);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsOk()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "newuser",
                Email = "newuser@test.com",
                Password = "password123",
                Name = "New User"
            };
            var expectedResponse = new AuthResponse
            {
                Token = "valid-jwt-token",
                Username = "newuser",
                Email = "newuser@test.com",
                Role = "User",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
            _mockAuthService.Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Register(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task Register_WithExistingUsername_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "existinguser",
                Email = "test@test.com",
                Password = "password123",
                Name = "Test User"
            };
            _mockAuthService.Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((AuthResponse?)null);

            // Act
            var result = await _controller.Register(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
