using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Business.DTO;

namespace Business.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardKPIDTO> GetDashboardKPIsAsync(DateTime? date = null);
        Task<DashboardKPIDTO> GetTodayKPIsAsync();
        Task<DashboardKPIDTO> GetThisMonthKPIsAsync();
        Task<IEnumerable<TopProductDTO>> GetTopSellingProductsAsync(int count = 10);
        Task<IEnumerable<ProductDTO>> GetTopStockProductsAsync(int count = 10);
        Task<IEnumerable<ExpiryAlertDTO>> GetExpiringProductsAsync(int daysAhead = 30);
        Task<IEnumerable<SaleDTO>> GetRecentSalesAsync(int count = 5);
        Task<IEnumerable<StockAdjustmentDTO>> GetRecentAdjustmentsAsync(int count = 5);
        Task<decimal> GetTotalStockValueAsync();
        Task<decimal> GetTotalSalesDueAsync();
        Task<decimal> GetTotalInvoiceDueAsync();
        Task<int> GetLowStockItemsCountAsync(int threshold = 10);
        Task<int> GetExpiringItemsCountAsync(int daysAhead = 30);
    }
}
