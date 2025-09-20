using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Business.DTO;

namespace Business.Interfaces
{
    public interface IReportService
    {
        Task<ReportDTO> GenerateSalesReportAsync(DateTime startDate, DateTime endDate);
        Task<ReportDTO> GenerateInventoryReportAsync();
        Task<IEnumerable<TopProductDTO>> GetTopSellingProductsReportAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<ExpiryAlertDTO>> GetExpiryReportAsync();
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalProfitAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<SaleDTO>> GetDailySalesReportAsync(DateTime date);
        Task<IEnumerable<SaleDTO>> GetMonthlySalesReportAsync(int year, int month);
        Task<ReportDTO> GenerateCustomerReportAsync(Guid customerId, DateTime? startDate = null, DateTime? endDate = null);
        //Task<IEnumerable<AuditLogDTO>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
