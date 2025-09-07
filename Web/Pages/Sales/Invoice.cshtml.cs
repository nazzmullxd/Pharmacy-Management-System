using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Business.Interfaces;
using Business.DTO;

namespace Web.Pages.Sales
{
    public class InvoiceModel : PageModel
    {
        private readonly ISalesService _salesService;

        public InvoiceModel(ISalesService salesService)
        {
            _salesService = salesService;
        }

        [FromRoute]
        public Guid Id { get; set; }

        public SaleDTO? Sale { get; set; }

        public async Task<IActionResult> OnGet(Guid id)
        {
            Id = id;
            Sale = await _salesService.GetSaleByIdAsync(id);
            if (Sale == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}


