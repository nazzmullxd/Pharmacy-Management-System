using Microsoft.AspNetCore.Mvc.RazorPages;
using Business.Interfaces;
using Business.DTO;

namespace Web.Pages
{
    public class SupportModel : PageModel
    {
        private readonly ISupportTicketService _supportService;

        public SupportModel(ISupportTicketService supportService)
        {
            _supportService = supportService;
        }

        public IEnumerable<SupportTicketDTO> Tickets { get; set; } = new List<SupportTicketDTO>();

        public async Task OnGet()
        {
            Tickets = await _supportService.GetAllTicketsAsync();
        }
    }
}


