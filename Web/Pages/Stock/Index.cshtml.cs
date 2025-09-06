using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Stock
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IStockService _stockService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IProductService productService, IStockService stockService, ILogger<IndexModel> logger)
        {
            _productService = productService;
            _stockService = stockService;
            _logger = logger;
        }

        public IEnumerable<ProductDTO> StockItems { get; set; } = new List<ProductDTO>();
        public IEnumerable<ProductDTO> LowStockAlerts { get; set; } = new List<ProductDTO>();
        public IEnumerable<ExpiryAlertDTO> ExpiringAlerts { get; set; } = new List<ExpiryAlertDTO>();
        
        // Summary properties
        public int TotalProducts { get; set; }
        public int LowStockItems { get; set; }
        public int ExpiringItems { get; set; }
        public decimal TotalStockValue { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Load all products with stock information
                StockItems = await _productService.GetAllProductsAsync();
                
                // Load low stock products
                LowStockAlerts = await _productService.GetLowStockProductsAsync();
                
                // Load expiring products
                ExpiringAlerts = await _stockService.GetExpiryAlertsAsync(); // Next 30 days
                
                // Calculate summary statistics
                CalculateSummaryStatistics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stock data");
                StockItems = new List<ProductDTO>();
                LowStockAlerts = new List<ProductDTO>();
                ExpiringAlerts = new List<ExpiryAlertDTO>();
            }
        }

        private void CalculateSummaryStatistics()
        {
            try
            {
                TotalProducts = StockItems.Count();
                LowStockItems = LowStockAlerts.Count();
                ExpiringItems = ExpiringAlerts.Count();
                TotalStockValue = StockItems.Sum(p => p.TotalStock * p.UnitPrice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating summary statistics");
                TotalProducts = 0;
                LowStockItems = 0;
                ExpiringItems = 0;
                TotalStockValue = 0;
            }
        }
    }
}
