using Microsoft.AspNetCore.Mvc.RazorPages;
using Business.Interfaces;
using Business.DTO;

namespace Web.Pages.Sales
{
    public class InvoicesModel : PageModel
    {
        private readonly ISalesService _salesService;

        public InvoicesModel(ISalesService salesService)
        {
            _salesService = salesService;
        }

        public IEnumerable<SaleDTO> Invoices { get; set; } = new List<SaleDTO>();

        public async Task OnGet()
        {
            var allSales = await _salesService.GetAllSalesAsync();
            Invoices = allSales.OrderByDescending(s => s.SaleDate);
        }
    }
}


