using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Reports
{
    public class ProfitLossModel : PageModel
    {
        private readonly ISalesService _salesService;
        private readonly IProductService _productService;

        public ProfitLossModel(ISalesService salesService, IProductService productService)
        {
            _salesService = salesService;
            _productService = productService;
        }

        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public IEnumerable<SaleDTO> Sales { get; set; } = Enumerable.Empty<SaleDTO>();

        public async Task OnGet()
        {
            Sales = await _salesService.GetAllSalesAsync();
            TotalRevenue = Sales.Sum(s => s.TotalAmount);
            
            // Simplified cost calculation (in real app, this would come from purchase costs)
            var products = await _productService.GetAllProductsAsync();
            TotalCost = products.Sum(p => p.TotalStock * (p.UnitPrice * 0.7m)); // Assume 70% of retail is cost
            
            GrossProfit = TotalRevenue - TotalCost;
            ProfitMargin = TotalRevenue > 0 ? (GrossProfit / TotalRevenue) * 100 : 0;
        }
    }
}


