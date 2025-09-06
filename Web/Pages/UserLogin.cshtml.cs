using System.ComponentModel.DataAnnotations;
using Business.Services;
using Database.Model;
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

        public void OnGet()
        {
        }

        public async Task<IActionResult>
    OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var isAuthenticated = await _userService.AuthenticateUserAsync(Input.Email, Input.Password);

            if (!isAuthenticated)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // User is authenticated, redirect to dashboard

            return RedirectToPage("Index");
        }

        public class LoginInputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }
    }
}
