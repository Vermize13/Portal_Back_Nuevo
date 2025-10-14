using Microsoft.AspNetCore.Authorization;

namespace API.Middleware
{
    /// <summary>
    /// Custom authorization attribute to restrict access based on user roles.
    /// Usage: [RoleAuthorization("Admin", "ProductOwner")]
    /// </summary>
    public class RoleAuthorizationAttribute : AuthorizeAttribute
    {
        public RoleAuthorizationAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }
}
