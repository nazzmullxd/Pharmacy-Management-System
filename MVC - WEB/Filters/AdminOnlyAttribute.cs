using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MVC_WEB.Filters
{
    /// <summary>
    /// Restricts access to Admin users only.
    /// Must be used in conjunction with [Authenticated] attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AdminOnlyAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // First check if authenticated
            var auth = context.HttpContext.Session.GetString("auth");
            if (auth != "1")
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Then check if admin
            var role = context.HttpContext.Session.GetString("role");
            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}
