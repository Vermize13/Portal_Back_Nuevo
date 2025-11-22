using API.Middleware;
using API.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace API.Tests.Middleware
{
    public class HttpAuditMiddlewareTests
    {
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly Mock<ILogger<HttpAuditMiddleware>> _loggerMock;
        private readonly Mock<IAuditService> _auditServiceMock;
        private readonly HttpAuditMiddleware _middleware;

        public HttpAuditMiddlewareTests()
        {
            _nextMock = new Mock<RequestDelegate>();
            _loggerMock = new Mock<ILogger<HttpAuditMiddleware>>();
            _auditServiceMock = new Mock<IAuditService>();
            _middleware = new HttpAuditMiddleware(_nextMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task InvokeAsync_ShouldAuditHttpRequest()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/api/test";
            context.Response.StatusCode = 200;
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context, _auditServiceMock.Object);

            // Assert
            _nextMock.Verify(next => next(context), Times.Once);
            
            // Wait a bit for the async logging to complete
            await Task.Delay(100);
            
            _auditServiceMock.Verify(
                service => service.LogHttpRequestAsync(
                    null,
                    "GET",
                    "/api/test",
                    200,
                    It.IsAny<long>(),
                    "127.0.0.1",
                    It.IsAny<string>(),
                    It.IsAny<Guid>()),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithAuthenticatedUser_ShouldIncludeUserId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            context.Request.Method = "POST";
            context.Request.Path = "/api/users";
            context.Response.StatusCode = 201;
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");

            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context, _auditServiceMock.Object);

            // Wait a bit for the async logging to complete
            await Task.Delay(100);

            // Assert
            _auditServiceMock.Verify(
                service => service.LogHttpRequestAsync(
                    userId,
                    "POST",
                    "/api/users",
                    201,
                    It.IsAny<long>(),
                    "192.168.1.1",
                    It.IsAny<string>(),
                    It.IsAny<Guid>()),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_ShouldSkipSwaggerPaths()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/swagger/index.html";
            context.Response.StatusCode = 200;

            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context, _auditServiceMock.Object);

            // Wait a bit to ensure no async logging happens
            await Task.Delay(100);

            // Assert
            _nextMock.Verify(next => next(context), Times.Once);
            _auditServiceMock.Verify(
                service => service.LogHttpRequestAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid>()),
                Times.Never);
        }
    }
}
