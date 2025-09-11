using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Stock
{
    public class BatchesModel : PageModel
    {
        private readonly IStockService _stockService;

        public BatchesModel(IStockService stockService)
        {
            _stockService = stockService;
        }

        public IEnumerable<ProductBatchDTO> Batches { get; set; } = Enumerable.Empty<ProductBatchDTO>();

        public async Task OnGet()
        {
            Batches = await _stockService.GetAllProductBatchesAsync();
        }
    }
}


