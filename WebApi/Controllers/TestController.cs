using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Middleware;

namespace WebApi.Controllers
{
    /// <summary>
    /// Controller de prueba para demostrar autorización por roles
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// Endpoint público - sin autenticación requerida
        /// </summary>
        [HttpGet("public")]
        [AllowAnonymous]
        public IActionResult Public()
        {
            return Ok(new { message = "Este endpoint es público y no requiere autenticación" });
        }

        /// <summary>
        /// Endpoint protegido - requiere autenticación pero no rol específico
        /// </summary>
        [HttpGet("protected")]
        [Authorize]
        public IActionResult Protected()
        {
            var username = User.Identity?.Name ?? "Unknown";
            return Ok(new { 
                message = "Este endpoint requiere autenticación",
                user = username
            });
        }

        /// <summary>
        /// Endpoint solo para administradores
        /// </summary>
        [HttpGet("admin-only")]
        [RoleAuthorization("admin")]
        public IActionResult AdminOnly()
        {
            var username = User.Identity?.Name ?? "Unknown";
            return Ok(new { 
                message = "Este endpoint es solo para administradores",
                user = username
            });
        }

        /// <summary>
        /// Endpoint para administradores y Product Owners
        /// </summary>
        [HttpGet("management")]
        [RoleAuthorization("admin", "product_owner")]
        public IActionResult Management()
        {
            var username = User.Identity?.Name ?? "Unknown";
            var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role)
                           .Select(c => c.Value)
                           .ToList();
            return Ok(new { 
                message = "Este endpoint es para administradores y Product Owners",
                user = username,
                roles = roles
            });
        }

        /// <summary>
        /// Endpoint para todos los roles excepto Admin (ejemplo usando [Authorize])
        /// </summary>
        [HttpGet("developers")]
        [Authorize(Roles = "desarrollador,qa_tester,product_owner")]
        public IActionResult Developers()
        {
            var username = User.Identity?.Name ?? "Unknown";
            var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role)
                           .Select(c => c.Value)
                           .ToList();
            return Ok(new { 
                message = "Este endpoint es para developers, testers y product owners",
                user = username,
                roles = roles
            });
        }
    }
}
