using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IDashboardService _dashboardService;
        private readonly ISalesService _salesService;
        private readonly IProductService _productService;
        private readonly IStockService _stockService;

        public IndexModel(
            ILogger<IndexModel> logger,
            IDashboardService dashboardService,
            ISalesService salesService,
            IProductService productService,
            IStockService stockService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
            _salesService = salesService;
            _productService = productService;
            _stockService = stockService;
        }

        // Dashboard KPIs
        public decimal TodaySales { get; set; }
        public int TodayInvoices { get; set; }
        public decimal MonthlySales { get; set; }
        public int MonthlyInvoices { get; set; }
        public decimal StockValue { get; set; }
        public decimal OutstandingDues { get; set; }

        // Dashboard Data
        public IEnumerable<TopProductDTO> TopSellingProducts { get; set; } = new List<TopProductDTO>();
        public IEnumerable<ProductDTO> TopStockProducts { get; set; } = new List<ProductDTO>();
        public IEnumerable<SaleDTO> RecentSales { get; set; } = new List<SaleDTO>();
        public IEnumerable<ExpiryAlertDTO> ExpiringProducts { get; set; } = new List<ExpiryAlertDTO>();

        public async Task OnGetAsync()
        {
            try
            {
                // Load dashboard KPIs
                await LoadDashboardKPIs();
                
                // Load dashboard data
                await LoadDashboardData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                // Set default values to prevent errors
                SetDefaultValues();
            }
        }

        private async Task LoadDashboardKPIs()
        {
            try
            {
                var today = DateTime.Today;
                var monthStart = new DateTime(today.Year, today.Month, 1);

                // Get today's sales
                var todaySales = await _salesService.GetSalesByDateRangeAsync(today, today.AddDays(1));
                TodaySales = todaySales.Sum(s => s.TotalAmount);
                TodayInvoices = todaySales.Count();

                // Get this month's sales
                var monthlySales = await _salesService.GetSalesByDateRangeAsync(monthStart, today.AddDays(1));
                MonthlySales = monthlySales.Sum(s => s.TotalAmount);
                MonthlyInvoices = monthlySales.Count();

                // Get stock value
                var products = await _productService.GetAllProductsAsync();
                StockValue = products.Sum(p => p.TotalStock * p.UnitPrice);

                // Get outstanding dues (pending payments)
                var allSales = await _salesService.GetAllSalesAsync();
                OutstandingDues = allSales.Where(s => s.PaymentStatus == "Pending").Sum(s => s.TotalAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard KPIs");
                SetDefaultKPIs();
            }
        }

        private async Task LoadDashboardData()
        {
            try
            {
                // Load top selling products (last month)
                var monthStart = DateTime.UtcNow.AddMonths(-1);
                var monthEnd = DateTime.UtcNow;
                TopSellingProducts = await _salesService.GetTopSellingProductsAsync(10, monthStart, monthEnd);

                // Load top stock products
                var products = await _productService.GetAllProductsAsync();
                TopStockProducts = products.OrderByDescending(p => p.TotalStock).Take(10);

                // Load recent sales
                RecentSales = await _salesService.GetAllSalesAsync();
                RecentSales = RecentSales.OrderByDescending(s => s.SaleDate).Take(5);

                // Load expiring products (next 30 days)
                ExpiringProducts = await _stockService.GetExpiryAlertsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                SetDefaultData();
            }
        }

        private void SetDefaultValues()
        {
            SetDefaultKPIs();
            SetDefaultData();
        }

        private void SetDefaultKPIs()
        {
            TodaySales = 0;
            TodayInvoices = 0;
            MonthlySales = 0;
            MonthlyInvoices = 0;
            StockValue = 0;
            OutstandingDues = 0;
        }

        private void SetDefaultData()
        {
            TopSellingProducts = new List<TopProductDTO>();
            TopStockProducts = new List<ProductDTO>();
            RecentSales = new List<SaleDTO>();
            ExpiringProducts = new List<ExpiryAlertDTO>();
        }
    }
}
