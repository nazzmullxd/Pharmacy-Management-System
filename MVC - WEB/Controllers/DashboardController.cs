using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MVC_WEB.Filters;
using MVC_WEB.Models.ViewModels;

namespace MVC_WEB.Controllers
{
    [Authenticated]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IDashboardService _dashboardService;

        public DashboardController(
            ILogger<DashboardController> logger,
            IDashboardService dashboardService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                UserEmail = HttpContext.Session.GetString("userEmail") ?? "Guest",
                UserFullName = HttpContext.Session.GetString("userName") ?? "Guest",
                Role = HttpContext.Session.GetString("role") ?? "User"
            };

            try
            {
                // Load dashboard data
                var today = await _dashboardService.GetTodayKPIsAsync();
                var month = await _dashboardService.GetThisMonthKPIsAsync();
                var stockValue = await _dashboardService.GetTotalStockValueAsync();
                var dues = await _dashboardService.GetTotalSalesDueAsync();
                var topSelling = await _dashboardService.GetTopSellingProductsAsync(10);
                var topStock = await _dashboardService.GetTopStockProductsAsync(10);
                var recentSales = await _dashboardService.GetRecentSalesAsync(5);
                var expiring = await _dashboardService.GetExpiringProductsAsync(30);

                viewModel.TodayOrders = today?.TodaySaleOrdersCount ?? 0;
                viewModel.ThisMonthOrders = month?.ThisMonthSaleOrdersCount ?? 0;
                viewModel.StockValue = stockValue;
                viewModel.OutstandingDues = dues;
                viewModel.TopSellingProducts = topSelling ?? Enumerable.Empty<TopProductDTO>();
                viewModel.TopStockProducts = topStock ?? Enumerable.Empty<ProductDTO>();
                viewModel.RecentSales = recentSales ?? Enumerable.Empty<SaleDTO>();
                viewModel.ExpiringProducts = expiring ?? Enumerable.Empty<ExpiryAlertDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                TempData["ErrorMessage"] = "Error loading dashboard data. Some information may be incomplete.";
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
