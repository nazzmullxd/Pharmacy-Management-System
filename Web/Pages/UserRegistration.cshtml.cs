using System.ComponentModel.DataAnnotations;
using Business.Services;
using Database.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages
{
    public class UserRegistrationModel : PageModel
    {
        private readonly UserService _userService;

        public UserRegistrationModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public RegistrationInputModel Input { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new UserInfo
            {
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                Email = Input.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.Password), // Hash the password
                Role = "Employee", // Default role
                LastLoginDate = DateTime.UtcNow
            };

            await _userService.AddUserAsync(user);

            return RedirectToPage("UserLogin");
        }

        public class RegistrationInputModel
        {
            [Required]
            [StringLength(50)]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            [StringLength(50)]
            public string LastName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}