using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using API.Controllers;
using Domain.Entity;
using Repository.Repositories;
using FluentAssertions;

namespace API.Tests.Controllers
{
    /// <summary>
    /// Tests for User Management endpoints
    /// Covers RF1 - Gestión de Usuarios
    /// RF1.1: El sistema debe permitir crear, editar, eliminar y desactivar usuarios
    /// </summary>
    public class UsersControllerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ILogger<UsersController>> _mockLogger;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<UsersController>>();
            _controller = new UsersController(_mockUserRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetUsers_ReturnsOkWithUserList()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Username = "user1", Email = "user1@test.com" },
                new User { Id = Guid.NewGuid(), Username = "user2", Email = "user2@test.com" }
            };
            _mockUserRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(users);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedUsers = okResult!.Value as IEnumerable<User>;
            returnedUsers.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetUser_WithValidId_ReturnsOkWithUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@test.com",
                Name = "Test User"
            };
            _mockUserRepository.Setup(x => x.GetAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedUser = okResult!.Value as User;
            returnedUser.Should().NotBeNull();
            returnedUser!.Id.Should().Be(userId);
            returnedUser.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task GetUser_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserRepository.Setup(x => x.GetAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetUserByEmail_WithValidEmail_ReturnsOkWithUser()
        {
            // Arrange
            var email = "test@test.com";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = email,
                Name = "Test User"
            };
            _mockUserRepository.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserByEmail(email);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedUser = okResult!.Value as User;
            returnedUser.Should().NotBeNull();
            returnedUser!.Email.Should().Be(email);
        }

        [Fact]
        public async Task GetUserByEmail_WithInvalidEmail_ReturnsNotFound()
        {
            // Arrange
            var email = "nonexistent@test.com";
            _mockUserRepository.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.GetUserByEmail(email);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }
    }
}
