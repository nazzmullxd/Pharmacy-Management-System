using Microsoft.AspNetCore.Mvc.RazorPages;
using Business.Interfaces;
using Business.DTO;

namespace Web.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly IUserService _userService;

        public ProfileModel(IUserService userService)
        {
            _userService = userService;
        }

        public UserDTO? CurrentUser { get; set; }

        public async Task OnGet()
        {
            // Replace with actual user context when auth is wired
            var email = User?.Identity?.Name;
            if (!string.IsNullOrEmpty(email))
            {
                CurrentUser = await _userService.GetUserByEmailAsync(email);
            }
        }
    }
}


