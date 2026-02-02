using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Services;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/configuration")]
    [Authorize]
    public class SystemConfigurationController : ControllerBase
    {
        private readonly ISystemConfigurationService _configService;

        public SystemConfigurationController(ISystemConfigurationService configService)
        {
            _configService = configService;
        }

        [HttpGet]
        public async Task<ActionResult<Dictionary<string, string>>> GetConfiguration()
        {
            // Verify admin role if needed, or allow read-only for some settings?
            // Admin component is the one using this.
            // Users might need max file size but that's usually public or fetched on demand.
            // Let's restrict write to Admin.
            
            var configs = await _configService.GetAllPublicConfigsAsync();
            
            // Should verify if we need to merge with existing hardcoded defaults if DB is empty?
            // For now, we return what's in DB.
            return Ok(configs);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateConfiguration([FromBody] Dictionary<string, string> configs)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = userIdStr != null ? Guid.Parse(userIdStr) : null;
            
            // Check Admin role
             if (!User.IsInRole("admin") && !User.IsInRole("Admin")) 
             {
                 // We can rely on proper role check later or assume [Authorize(Roles="Admin")]
                 // But for now, let's just save.
                 // Actually, let's enforce Admin here.
                 // The project has roles "Admin" vs "admin", we fixed ProjectsController.
                 // Let's check roles claims.
                 var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
                 if (!string.Equals(roleClaim, "admin", StringComparison.OrdinalIgnoreCase))
                 {
                     return Forbid();
                 }
             }

            foreach (var kvp in configs)
            {
                await _configService.SetValueAsync(kvp.Key, kvp.Value, userId: userId);
            }

            return Ok();
        }
        
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetPublicConfiguration()
        {
            // Endpoint for non-auth users or general usage (file size limits etc)
            var maxSize = await _configService.GetIntAsync("MaxUploadSizeMB", 10);
            return Ok(new { maxUploadSize = maxSize });
        }
    }
}
