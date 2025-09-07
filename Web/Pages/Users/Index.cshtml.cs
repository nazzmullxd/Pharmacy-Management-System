using Microsoft.AspNetCore.Mvc.RazorPages;
using Business.Interfaces;
using Business.DTO;

namespace Web.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;

        public IndexModel(IUserService userService)
        {
            _userService = userService;
        }

        public IEnumerable<UserDTO> Users { get; set; } = new List<UserDTO>();

        public async Task OnGet()
        {
            Users = await _userService.GetAllUsersAsync();
        }
    }
}


