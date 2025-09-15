using System;
using System.Linq;
using System.Threading.Tasks;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IDashboardService _dashboardService;

        public IndexModel(ILogger<IndexModel> logger, IDashboardService dashboardService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
        }

        public string? UserEmail { get; private set; }
        public string Role { get; private set; } = "Employee";

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("auth") != "1")
                return RedirectToPage("UserLogin");

            UserEmail = HttpContext.Session.GetString("userEmail");
            if (string.IsNullOrEmpty(UserEmail))
            {
                HttpContext.Session.Clear();
                return RedirectToPage("UserLogin");
            }

            Role = HttpContext.Session.GetString("role") ?? "Employee";
            return Page();
        }

        public async Task<IActionResult> OnGetKpisAsync()
        {
            try
            {
                var todayTask = _dashboardService.GetTodayKPIsAsync();
                var monthTask = _dashboardService.GetThisMonthKPIsAsync();
                var stockValueTask = _dashboardService.GetTotalStockValueAsync();
                var outstandingTask = _dashboardService.GetTotalSalesDueAsync();

                await Task.WhenAll(todayTask, monthTask, stockValueTask, outstandingTask);

                return new JsonResult(new
                {
                    todayOrders = todayTask.Result?.TodaySaleOrdersCount ?? 0,
                    monthOrders = monthTask.Result?.ThisMonthSaleOrdersCount ?? 0,
                    stockValue = Math.Round(stockValueTask.Result, 2),
                    outstandingDues = Math.Round(outstandingTask.Result, 2)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving KPIs");
                return StatusCode(500, new { error = "Failed to load dashboard data" });
            }
        }

        public async Task<IActionResult> OnGetStockStatusDataAsync()
        {
            try
            {
                var products = await _dashboardService.GetTopStockProductsAsync(10)
                               ?? Enumerable.Empty<Business.DTO.ProductDTO>();

                var validProducts = products
                    .Where(p => p != null && !string.IsNullOrEmpty(p.ProductName))
                    .Take(10)
                    .ToList();

                return new JsonResult(new
                {
                    labels = validProducts.Select(l => l.ProductName).ToArray(),
                    values = validProducts.Select(l => l.TotalStock).ToArray()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stock status");
                return new JsonResult(new { labels = Array.Empty<string>(), values = Array.Empty<int>() });
            }
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("UserLogin");
        }
    }
}