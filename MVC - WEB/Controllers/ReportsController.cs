using Microsoft.AspNetCore.Mvc;
using Business.Interfaces;
using Business.DTO;
using MVC_WEB.Filters;
using MVC_WEB.Models.ViewModels;
using Database.Interfaces;

namespace MVC_WEB.Controllers
{
    [Authenticated]
    public class ReportsController : Controller
    {
        private readonly ISalesService _salesService;
        private readonly IProductService _productService;
        private readonly IStockService _stockService;
        private readonly IDashboardService _dashboardService;
        private readonly IAntibioticLogRepository _antibioticLogRepository;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            ISalesService salesService,
            IProductService productService,
            IStockService stockService,
            IDashboardService dashboardService,
            IAntibioticLogRepository antibioticLogRepository,
            ILogger<ReportsController> logger)
        {
            _salesService = salesService;
            _productService = productService;
            _stockService = stockService;
            _dashboardService = dashboardService;
            _antibioticLogRepository = antibioticLogRepository;
            _logger = logger;
        }

        // GET: Reports (Dashboard)
        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = await _dashboardService.GetDashboardKPIsAsync();

                var viewModel = new ReportsIndexViewModel
                {
                    TodaysSales = dashboardData?.TodaySalesAmount ?? 0,
                    LowStockItems = dashboardData?.LowStockItemsCount ?? 0,
                    ExpiringItems = dashboardData?.ExpiringItemsCount ?? 0,
                    TotalCustomers = dashboardData?.TopSellingProductsCount ?? 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reports dashboard");
                return View(new ReportsIndexViewModel());
            }
        }

        // GET: Reports/Sales
        public async Task<IActionResult> Sales(string? type, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var userRole = HttpContext.Session.GetString("role") ?? "User";
                
                // All authenticated users can access Sales reports
                var sales = await _salesService.GetAllSalesAsync();
                var topProducts = await _salesService.GetTopSellingProductsAsync(10);

                var salesList = sales.ToList();
                var today = DateTime.Today;
                var monthStart = new DateTime(today.Year, today.Month, 1);

                var viewModel = new SalesReportViewModel
                {
                    Sales = salesList,
                    TopProducts = topProducts,
                    TotalSales = salesList.Sum(s => s.TotalAmount),
                    TotalInvoices = salesList.Count,
                    TodaySales = salesList.Where(s => s.SaleDate.Date == today).Sum(s => s.TotalAmount),
                    MonthlySales = salesList.Where(s => s.SaleDate >= monthStart).Sum(s => s.TotalAmount),
                    ReportType = type,
                    StartDate = startDate,
                    EndDate = endDate
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sales report");
                TempData["Error"] = "Failed to load sales report.";
                return View(new SalesReportViewModel());
            }
        }

        // GET: Reports/Stock (Admin only)
        [AdminOnly]
        public async Task<IActionResult> Stock(string? stockFilter, string? expiryFilter)
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                var batches = await _stockService.GetAllProductBatchesAsync();
                var expiringItems = await _stockService.GetExpiryAlertsAsync();

                var productList = products.ToList();

                var viewModel = new StockReportViewModel
                {
                    Products = productList,
                    Batches = batches,
                    ExpiringItems = expiringItems,
                    TotalStockValue = productList.Sum(p => p.TotalStock * p.UnitPrice),
                    LowStockCount = productList.Count(p => p.IsLowStock),
                    StockFilter = stockFilter,
                    ExpiryFilter = expiryFilter
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stock report");
                TempData["Error"] = "Failed to load stock report.";
                return View(new StockReportViewModel());
            }
        }

        // GET: Reports/ProfitLoss (Admin only)
        [AdminOnly]
        public async Task<IActionResult> ProfitLoss(string? period, string? reportType)
        {
            try
            {
                var sales = await _salesService.GetAllSalesAsync();
                var products = await _productService.GetAllProductsAsync();

                var salesList = sales.ToList();
                var totalRevenue = salesList.Sum(s => s.TotalAmount);
                
                // Simplified cost calculation (in real app, this would come from purchase costs)
                var totalCost = products.Sum(p => p.TotalStock * (p.UnitPrice * 0.7m)); // Assume 70% of retail is cost
                var grossProfit = totalRevenue - totalCost;
                var profitMargin = totalRevenue > 0 ? (grossProfit / totalRevenue) * 100 : 0;

                var viewModel = new ProfitLossReportViewModel
                {
                    TotalRevenue = totalRevenue,
                    TotalCost = totalCost,
                    GrossProfit = grossProfit,
                    ProfitMargin = profitMargin,
                    Sales = salesList,
                    PeriodFilter = period,
                    ReportType = reportType
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profit/loss report");
                TempData["Error"] = "Failed to load profit/loss report.";
                return View(new ProfitLossReportViewModel());
            }
        }

        // GET: Reports/Antibiotic (Admin only - regulatory compliance)
        [AdminOnly]
        public async Task<IActionResult> Antibiotic(string? dateFilter, string? productFilter)
        {
            try
            {
                // Note: This would need to be implemented in the repository/service layer
                // For now, return empty collection as placeholder
                var antibioticLogs = new List<AntibioticLogDTO>();

                var viewModel = new AntibioticReportViewModel
                {
                    AntibioticLogs = antibioticLogs,
                    DateFilter = dateFilter,
                    ProductFilter = productFilter
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading antibiotic report");
                TempData["Error"] = "Failed to load antibiotic report.";
                return View(new AntibioticReportViewModel());
            }
        }
    }
}
