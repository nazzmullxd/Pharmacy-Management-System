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

            var users = await _userService.SearchUsersByEmailAsync(Input.Email);
            var user = users.FirstOrDefault();

            if (user == null || !BCrypt.Net.BCrypt.Verify(Input.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // Update last login date
            user.LastLoginDate = DateTime.UtcNow;
            await _userService.UpdateUserAsync(user);

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
