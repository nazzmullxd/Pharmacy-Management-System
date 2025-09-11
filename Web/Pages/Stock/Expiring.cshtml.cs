using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Stock
{
    public class ExpiringModel : PageModel
    {
        private readonly IStockService _stockService;

        public ExpiringModel(IStockService stockService)
        {
            _stockService = stockService;
        }

        public IEnumerable<ProductBatchDTO> Batches { get; set; } = Enumerable.Empty<ProductBatchDTO>();

        public async Task OnGet()
        {
            Batches = await _stockService.GetExpiringBatchesAsync(30);
        }
    }
}


