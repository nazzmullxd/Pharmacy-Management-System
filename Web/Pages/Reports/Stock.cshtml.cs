using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Reports
{
    public class StockModel : PageModel
    {
        private readonly IStockService _stockService;
        private readonly IProductService _productService;

        public StockModel(IStockService stockService, IProductService productService)
        {
            _stockService = stockService;
            _productService = productService;
        }

        public IEnumerable<ProductDTO> Products { get; set; } = Enumerable.Empty<ProductDTO>();
        public IEnumerable<ProductBatchDTO> Batches { get; set; } = Enumerable.Empty<ProductBatchDTO>();
        public IEnumerable<ExpiryAlertDTO> ExpiringItems { get; set; } = Enumerable.Empty<ExpiryAlertDTO>();
        public decimal TotalStockValue { get; set; }
        public int LowStockCount { get; set; }

        public async Task OnGet()
        {
            Products = await _productService.GetAllProductsAsync();
            Batches = await _stockService.GetAllProductBatchesAsync();
            ExpiringItems = await _stockService.GetExpiryAlertsAsync();
            
            TotalStockValue = Products.Sum(p => p.TotalStock * p.UnitPrice);
            LowStockCount = Products.Count(p => p.IsLowStock);
        }
    }
}


