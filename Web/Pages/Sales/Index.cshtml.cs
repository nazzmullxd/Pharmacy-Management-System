using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Sales
{
    public class IndexModel : PageModel
    {
        private readonly ISalesService _salesService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ISalesService salesService, ILogger<IndexModel> logger)
        {
            _salesService = salesService;
            _logger = logger;
        }

        public IEnumerable<SaleDTO> Sales { get; set; } = new List<SaleDTO>();
        
        // Summary properties
        public decimal TodaySales { get; set; }
        public int TodaySalesCount { get; set; }
        public decimal MonthlySales { get; set; }
        public int MonthlySalesCount { get; set; }
        public int PendingOrders { get; set; }
        public int TotalOrders { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Load recent sales (last 50)
                Sales = await _salesService.GetAllSalesAsync();
                Sales = Sales.Take(50).ToList();

                // Calculate summary statistics
                await CalculateSummaryStatistics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sales data");
                Sales = new List<SaleDTO>();
            }
        }

        private async Task CalculateSummaryStatistics()
        {
            try
            {
                var today = DateTime.Today;
                var monthStart = new DateTime(today.Year, today.Month, 1);

                // Get today's sales
                var todaySales = await _salesService.GetSalesByDateRangeAsync(today, today.AddDays(1));
                TodaySales = todaySales.Sum(s => s.TotalAmount);
                TodaySalesCount = todaySales.Count();

                // Get this month's sales
                var monthlySales = await _salesService.GetSalesByDateRangeAsync(monthStart, today.AddDays(1));
                MonthlySales = monthlySales.Sum(s => s.TotalAmount);
                MonthlySalesCount = monthlySales.Count();

                // Get pending orders
                var allSales = await _salesService.GetAllSalesAsync();
                PendingOrders = allSales.Count(s => s.PaymentStatus == "Pending");
                TotalOrders = allSales.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating summary statistics");
                // Set default values
                TodaySales = 0;
                TodaySalesCount = 0;
                MonthlySales = 0;
                MonthlySalesCount = 0;
                PendingOrders = 0;
                TotalOrders = 0;
            }
        }

        public async Task<IActionResult> OnPostMarkAsPaidAsync(Guid saleId)
        {
            try
            {
                await _salesService.UpdatePaymentStatusAsync(saleId, "Paid");
                TempData["SuccessMessage"] = "Sale marked as paid successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for sale {SaleId}", saleId);
                TempData["ErrorMessage"] = "Error updating payment status. Please try again.";
            }

            return RedirectToPage();
        }
    }
}
