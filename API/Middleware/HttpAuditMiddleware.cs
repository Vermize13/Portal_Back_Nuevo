using System.Diagnostics;
using System.Security.Claims;
using API.Services;

namespace API.Middleware
{
    /// <summary>
    /// Middleware to audit HTTP requests and responses
    /// </summary>
    public class HttpAuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpAuditMiddleware> _logger;
        private readonly HashSet<string> _excludedPaths;

        public HttpAuditMiddleware(RequestDelegate next, ILogger<HttpAuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            
            // Paths to exclude from auditing
            _excludedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "/swagger",
                "/health",
                "/favicon.ico"
            };
        }

        public async Task InvokeAsync(HttpContext context, IAuditService auditService)
        {
            // Skip auditing for excluded paths
            if (_excludedPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
            {
                await _next(context);
                return;
            }

            var requestId = Guid.NewGuid();
            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            try
            {
                // Continue processing the request
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                try
                {
                    // Extract user information if authenticated
                    Guid? actorId = null;
                    if (context.User?.Identity?.IsAuthenticated == true)
                    {
                        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
                        {
                            actorId = parsedUserId;
                        }
                    }

                    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                    var userAgent = context.Request.Headers["User-Agent"].ToString();
                    var httpMethod = context.Request.Method;
                    var httpPath = context.Request.Path.ToString();
                    var statusCode = context.Response.StatusCode;
                    var durationMs = stopwatch.ElapsedMilliseconds;

                    // Log the HTTP request asynchronously without blocking the response
                    // Note: Task.Run is used for simplicity. For high-traffic applications,
                    // consider using a background service with a queue to avoid thread pool exhaustion
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await auditService.LogHttpRequestAsync(actorId, httpMethod, httpPath, statusCode, durationMs, ipAddress, userAgent, requestId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to log HTTP request audit");
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in HttpAuditMiddleware");
                }
            }
        }
    }

    public static class HttpAuditMiddlewareExtensions
    {
        public static IApplicationBuilder UseHttpAuditing(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HttpAuditMiddleware>();
        }
    }
}
