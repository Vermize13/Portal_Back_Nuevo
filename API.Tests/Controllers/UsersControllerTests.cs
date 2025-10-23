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
            var role1 = new Role { Id = Guid.NewGuid(), Code = "Admin", Name = "Administrator" };
            var role2 = new Role { Id = Guid.NewGuid(), Code = "User", Name = "User" };
            
            var user1 = new User 
            { 
                Id = Guid.NewGuid(), 
                Username = "user1", 
                Email = "user1@test.com",
                UserRoles = new List<UserRole>
                {
                    new UserRole { RoleId = role1.Id, Role = role1, AssignedAt = DateTimeOffset.UtcNow }
                }
            };
            var user2 = new User 
            { 
                Id = Guid.NewGuid(), 
                Username = "user2", 
                Email = "user2@test.com",
                UserRoles = new List<UserRole>
                {
                    new UserRole { RoleId = role2.Id, Role = role2, AssignedAt = DateTimeOffset.UtcNow }
                }
            };
            
            var users = new List<User> { user1, user2 };
            _mockUserRepository.Setup(x => x.GetAllUsersWithRolesAsync()).ReturnsAsync(users);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedUsers = okResult!.Value as IEnumerable<User>;
            returnedUsers.Should().HaveCount(2);
            returnedUsers!.First().UserRoles.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetUser_WithValidId_ReturnsOkWithUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var role = new Role { Id = Guid.NewGuid(), Code = "Admin", Name = "Administrator" };
            var user = new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@test.com",
                Name = "Test User",
                UserRoles = new List<UserRole>
                {
                    new UserRole { UserId = userId, RoleId = role.Id, Role = role, AssignedAt = DateTimeOffset.UtcNow }
                }
            };
            _mockUserRepository.Setup(x => x.GetByIdWithRolesAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedUser = okResult!.Value as User;
            returnedUser.Should().NotBeNull();
            returnedUser!.Id.Should().Be(userId);
            returnedUser.Username.Should().Be("testuser");
            returnedUser.UserRoles.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetUser_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserRepository.Setup(x => x.GetByIdWithRolesAsync(userId)).ReturnsAsync((User?)null);

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
