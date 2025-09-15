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
                // Pull all sales once (consider future pagination)
                var all = await _salesService.GetAllSalesAsync();
                // Display only most recent 50 (order by date desc)
                Sales = all
                    .OrderByDescending(s => s.SaleDate)
                    .Take(50)
                    .ToList();

                CalculateSummaryStatistics(all);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sales data");
                Sales = new List<SaleDTO>();
                ResetSummaries();
            }
        }

        private void CalculateSummaryStatistics(IEnumerable<SaleDTO> allSales)
        {
            try
            {
                // Use UTC boundaries because SaleDate stored as UTC
                var utcToday = DateTime.UtcNow.Date;
                var utcMonthStart = new DateTime(utcToday.Year, utcToday.Month, 1);

                var todaySlice = allSales.Where(s => s.SaleDate >= utcToday && s.SaleDate < utcToday.AddDays(1)).ToList();
                TodaySales = todaySlice.Sum(s => s.TotalAmount);
                TodaySalesCount = todaySlice.Count;

                var monthSlice = allSales.Where(s => s.SaleDate >= utcMonthStart && s.SaleDate < utcToday.AddDays(1)).ToList();
                MonthlySales = monthSlice.Sum(s => s.TotalAmount);
                MonthlySalesCount = monthSlice.Count;

                PendingOrders = allSales.Count(s => s.PaymentStatus == SaleDTO.PaymentStatuses.Pending);
                TotalOrders = allSales.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating summary statistics");
                ResetSummaries();
            }
        }

        private void ResetSummaries()
        {
            TodaySales = 0;
            TodaySalesCount = 0;
            MonthlySales = 0;
            MonthlySalesCount = 0;
            PendingOrders = 0;
            TotalOrders = 0;
        }

        // Server-side action to mark as paid (ensure form posts here; JS alone should not mutate UI)
        public async Task<IActionResult> OnPostMarkAsPaidAsync(Guid saleId)
        {
            if (saleId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Invalid sale identifier.";
                return RedirectToPage();
            }

            try
            {
                var ok = await _salesService.UpdatePaymentStatusAsync(saleId, SaleDTO.PaymentStatuses.Paid);
                TempData[ok ? "SuccessMessage" : "ErrorMessage"] =
                    ok ? "Sale marked as paid successfully." : "Failed to update payment status.";
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