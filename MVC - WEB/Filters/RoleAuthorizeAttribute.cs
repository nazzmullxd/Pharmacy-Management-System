using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MVC_WEB.Filters
{
    /// <summary>
    /// Restricts access to specified roles.
    /// Usage: [RoleAuthorize("Admin", "Manager")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // First check if authenticated
            var auth = context.HttpContext.Session.GetString("auth");
            if (auth != "1")
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Then check if user has one of the allowed roles
            var userRole = context.HttpContext.Session.GetString("role");
            
            if (string.IsNullOrEmpty(userRole) || 
                !_allowedRoles.Any(r => string.Equals(r, userRole, StringComparison.OrdinalIgnoreCase)))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}
