using Microsoft.AspNetCore.Mvc.RazorPages;
using Business.Interfaces;
using System.Threading.Tasks;

namespace Web.Pages.Reports
{
    public class IndexModel : PageModel
    {
        private readonly IDashboardService _dashboardService;

        public IndexModel(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // Quick stats properties
        public decimal? TodaysSales { get; set; }
        public int? LowStockItems { get; set; }
        public int? ExpiringItems { get; set; }
        public int? TotalCustomers { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Load quick statistics for the dashboard
                var dashboardData = await _dashboardService.GetDashboardKPIsAsync();
                
                TodaysSales = dashboardData?.TodaySalesAmount;
                LowStockItems = dashboardData?.LowStockItemsCount;
                ExpiringItems = dashboardData?.ExpiringItemsCount;
                TotalCustomers = dashboardData?.TopSellingProductsCount; // Placeholder until proper customer count is available
            }
            catch (System.Exception)
            {
                // If dashboard service fails, show zeros
                TodaysSales = 0;
                LowStockItems = 0;
                ExpiringItems = 0;
                TotalCustomers = 0;
            }
        }
    }
}