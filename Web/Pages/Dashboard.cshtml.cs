using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Web.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ILogger<DashboardModel> _logger;
        private readonly IDashboardService _dashboardService;

        public DashboardModel(
            ILogger<DashboardModel> logger,
            IDashboardService dashboardService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
        }

        // Session / identity
        public string? UserEmail { get; private set; }
        public string Role { get; private set; } = "Employee";

        // KPI values
        public int TodayOrders { get; private set; }
        public int ThisMonthOrders { get; private set; }
        public decimal StockValue { get; private set; }
        public decimal OutstandingDues { get; private set; }

        // Lists
        public IEnumerable<TopProductDTO> TopSellingProducts { get; private set; } = Enumerable.Empty<TopProductDTO>();
        public IEnumerable<ProductDTO> TopStockProducts { get; private set; } = Enumerable.Empty<ProductDTO>();
        public IEnumerable<SaleDTO> RecentSales { get; private set; } = Enumerable.Empty<SaleDTO>();
        public IEnumerable<ExpiryAlertDTO> ExpiringProducts { get; private set; } = Enumerable.Empty<ExpiryAlertDTO>();

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetString("auth") != "1")
                return RedirectToPage("UserLogin");

            UserEmail = HttpContext.Session.GetString("userEmail");
            Role = HttpContext.Session.GetString("role") ?? "Employee";

            try
            {
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                SetDefaults();
            }

            return Page();
        }

        private async Task LoadDataAsync()
        {
            // Sequentially await each call to avoid DbContext concurrency issues
            var today = await _dashboardService.GetTodayKPIsAsync();
            var month = await _dashboardService.GetThisMonthKPIsAsync();
            var stockValue = await _dashboardService.GetTotalStockValueAsync();
            var dues = await _dashboardService.GetTotalSalesDueAsync();
            var topSelling = await _dashboardService.GetTopSellingProductsAsync(10);
            var topStock = await _dashboardService.GetTopStockProductsAsync(10);
            var recentSales = await _dashboardService.GetRecentSalesAsync(5);
            var expiring = await _dashboardService.GetExpiringProductsAsync(30);

            TodayOrders      = today?.TodaySaleOrdersCount ?? 0;
            ThisMonthOrders  = month?.ThisMonthSaleOrdersCount ?? 0;
            StockValue       = stockValue;
            OutstandingDues  = dues;

            TopSellingProducts = topSelling ?? Enumerable.Empty<TopProductDTO>();
            TopStockProducts   = topStock ?? Enumerable.Empty<ProductDTO>();
            RecentSales        = recentSales ?? Enumerable.Empty<SaleDTO>();
            ExpiringProducts   = expiring ?? Enumerable.Empty<ExpiryAlertDTO>();
        }

        private void SetDefaults()
        {
            TodayOrders = 0;
            ThisMonthOrders = 0;
            StockValue = 0;
            OutstandingDues = 0;
            TopSellingProducts = Enumerable.Empty<TopProductDTO>();
            TopStockProducts = Enumerable.Empty<ProductDTO>();
            RecentSales = Enumerable.Empty<SaleDTO>();
            ExpiringProducts = Enumerable.Empty<ExpiryAlertDTO>();
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("UserLogin");
        }
    }
}