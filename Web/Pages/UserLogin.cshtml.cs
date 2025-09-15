using System.ComponentModel.DataAnnotations;
using Business.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages
{
    public class UserLoginModel : PageModel
    {
        private readonly UserService _userService;

        public UserLoginModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        public async Task<IActionResult> OnGet()
        {
            if (HttpContext.Session.GetString("auth") == "1")
            {
                return RedirectToPage("Dashboard");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Fetch user first
            var user = await _userService.GetUserByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            var valid = await _userService.AuthenticateUserAsync(Input.Email, Input.Password);
            if (!valid)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            await _userService.UpdateLastLoginAsync(user.UserID);

            // Session-based (temporary) "auth"
            HttpContext.Session.SetString("auth", "1");
            HttpContext.Session.SetString("userEmail", user.Email);
            HttpContext.Session.SetString("role", user.Role);

            // Example role-based redirect (adjust if you add separate admin page)
            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("Dashboard");
            }

            return RedirectToPage("Dashboard");
        }

        public class LoginInputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }
    }
}