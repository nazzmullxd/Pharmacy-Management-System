using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MVC_WEB.Filters
{
    /// <summary>
    /// Ensures the user is authenticated via session.
    /// Redirects to login page if not authenticated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AuthenticatedAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var auth = context.HttpContext.Session.GetString("auth");
            
            if (auth != "1")
            {
                // User is not authenticated, redirect to login
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
        }
    }
}
