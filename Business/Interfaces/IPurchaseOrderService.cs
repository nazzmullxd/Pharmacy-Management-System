using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Business.DTO;

namespace Business.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task<IEnumerable<PurchaseOrderDTO>> GetAllPurchaseOrdersAsync();
        Task<PurchaseOrderDTO?> GetPurchaseOrderByIdAsync(Guid orderId);
        Task<PurchaseOrderDTO> CreatePurchaseOrderAsync(PurchaseOrderDTO orderDto);
        Task<PurchaseOrderDTO> UpdatePurchaseOrderAsync(PurchaseOrderDTO orderDto);
        Task<bool> DeletePurchaseOrderAsync(Guid orderId);
        Task<IEnumerable<PurchaseOrderDTO>> GetOrdersBySupplierAsync(Guid supplierId);
        Task<IEnumerable<PurchaseOrderDTO>> GetOrdersByStatusAsync(string status);
        Task<IEnumerable<PurchaseOrderDTO>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<bool> ApprovePurchaseOrderAsync(Guid orderId, Guid approvedBy);
        Task<bool> ReceivePurchaseOrderAsync(Guid orderId, List<PurchaseOrderItemDTO> receivedItems);
        Task<bool> CancelPurchaseOrderAsync(Guid orderId, string reason);
        Task<bool> ProcessPurchaseOrderAsync(Guid orderId, Guid processedBy);
        Task<string> GenerateOrderNumberAsync();
        Task<decimal> GetTotalOrderValueAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<PurchaseOrderDTO>> GetPendingOrdersAsync();
        Task<IEnumerable<PurchaseOrderDTO>> GetOverdueOrdersAsync();
    }
}
