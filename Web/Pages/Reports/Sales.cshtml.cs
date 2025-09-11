using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Reports
{
    public class SalesModel : PageModel
    {
        private readonly ISalesService _salesService;

        public SalesModel(ISalesService salesService)
        {
            _salesService = salesService;
        }

        public IEnumerable<SaleDTO> Sales { get; set; } = Enumerable.Empty<SaleDTO>();
        public IEnumerable<TopProductDTO> TopProducts { get; set; } = Enumerable.Empty<TopProductDTO>();
        public decimal TotalSales { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TodaySales { get; set; }
        public decimal MonthlySales { get; set; }

        public async Task OnGet()
        {
            Sales = await _salesService.GetAllSalesAsync();
            TopProducts = await _salesService.GetTopSellingProductsAsync(10);
            
            TotalSales = Sales.Sum(s => s.TotalAmount);
            TotalInvoices = Sales.Count();
            
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            
            TodaySales = Sales.Where(s => s.SaleDate.Date == today).Sum(s => s.TotalAmount);
            MonthlySales = Sales.Where(s => s.SaleDate >= monthStart).Sum(s => s.TotalAmount);
        }
    }
}


