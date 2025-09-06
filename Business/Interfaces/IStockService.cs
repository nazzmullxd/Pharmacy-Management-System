using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Business.DTO;

namespace Business.Interfaces
{
    public interface IStockService
    {
        Task<IEnumerable<ProductBatchDTO>> GetAllProductBatchesAsync();
        Task<ProductBatchDTO?> GetProductBatchByIdAsync(Guid batchId);
        Task<ProductBatchDTO> CreateProductBatchAsync(ProductBatchDTO batchDto);
        Task<ProductBatchDTO> UpdateProductBatchAsync(ProductBatchDTO batchDto);
        Task<bool> DeleteProductBatchAsync(Guid batchId);
        Task<IEnumerable<ProductBatchDTO>> GetBatchesByProductAsync(Guid productId);
        Task<IEnumerable<ProductBatchDTO>> GetExpiringBatchesAsync(int daysAhead = 30);
        Task<IEnumerable<ProductBatchDTO>> GetExpiredBatchesAsync();
        Task<IEnumerable<ExpiryAlertDTO>> GetExpiryAlertsAsync();
        Task<int> GetTotalStockForProductAsync(Guid productId);
        Task<bool> AdjustStockAsync(Guid batchId, int quantityChange, string reason);
        Task<IEnumerable<ProductBatchDTO>> GetLowStockBatchesAsync(int threshold = 10);
        Task<bool> ProcessStockAdjustmentAsync(Guid batchId, int newQuantity, string reason);
    }
}
