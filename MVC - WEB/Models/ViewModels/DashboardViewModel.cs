using Business.DTO;

namespace MVC_WEB.Models.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        // KPI values
        public int TodayOrders { get; set; }
        public int ThisMonthOrders { get; set; }
        public decimal StockValue { get; set; }
        public decimal OutstandingDues { get; set; }

        // Lists
        public IEnumerable<TopProductDTO> TopSellingProducts { get; set; } = Enumerable.Empty<TopProductDTO>();
        public IEnumerable<ProductDTO> TopStockProducts { get; set; } = Enumerable.Empty<ProductDTO>();
        public IEnumerable<SaleDTO> RecentSales { get; set; } = Enumerable.Empty<SaleDTO>();
        public IEnumerable<ExpiryAlertDTO> ExpiringProducts { get; set; } = Enumerable.Empty<ExpiryAlertDTO>();
    }
}
