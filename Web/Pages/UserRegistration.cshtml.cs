using System.ComponentModel.DataAnnotations;
using Business.DTO;
using Business.Services;
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

        public async Task<IActionResult> OnGet()
        {
            // If already logged in, send to dashboard
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

            // Email uniqueness check
            var isUnique = await _userService.IsEmailUniqueAsync(Input.Email);
            if (!isUnique)
            {
                ModelState.AddModelError("Input.Email", "This email is already registered.");
                return Page();
            }

            // Determine role (first user becomes Admin; others Employee)
            var allUsers = await _userService.GetAllUsersAsync();
            var assignedRole = allUsers.Any() ? "Employee" : "Admin";

            var userDto = new UserDTO
            {
                UserID = Guid.NewGuid(),
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                Email = Input.Email,
                Role = assignedRole,
                LastLoginDate = DateTime.UtcNow
            };

            await _userService.CreateUserAsync(userDto, Input.Password);

            // Optionally auto-login after registration
            HttpContext.Session.SetString("auth", "1");
            HttpContext.Session.SetString("userEmail", userDto.Email);
            HttpContext.Session.SetString("role", userDto.Role);

            return RedirectToPage("Dashboard");
        }

        public class RegistrationInputModel
        {
            [Required, StringLength(50)]
            public string FirstName { get; set; } = string.Empty;

            [Required, StringLength(50)]
            public string LastName { get; set; } = string.Empty;

            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, DataType(DataType.Password), MinLength(8)]
            public string Password { get; set; } = string.Empty;

            [Required, DataType(DataType.Password)]
            [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}