using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Business.DTO;

namespace Business.Interfaces
{
    public interface ISalesService
    {
        Task<IEnumerable<SaleDTO>> GetAllSalesAsync();
        Task<SaleDTO?> GetSaleByIdAsync(Guid saleId);
        Task<SaleDTO> CreateSaleAsync(SaleDTO saleDto);
        Task<SaleDTO> UpdateSaleAsync(SaleDTO saleDto);
        Task<bool> DeleteSaleAsync(Guid saleId);
        Task<IEnumerable<SaleDTO>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<SaleDTO>> GetSalesByCustomerAsync(Guid customerId);
        Task<IEnumerable<SaleDTO>> GetSalesByUserAsync(Guid userId);
        Task<decimal> GetTotalSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<TopProductDTO>> GetTopSellingProductsAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null);
        Task<bool> ProcessSaleAsync(SaleDTO saleDto);
        Task<bool> UpdatePaymentStatusAsync(Guid saleId, string paymentStatus);
    }
}
