using Database.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Interfaces
{
    public interface IStockAdjustmentRepository
    {
        Task<IEnumerable<StockAdjustment>> GetAllAsync();
        Task<StockAdjustment?> GetByIdAsync(Guid stockAdjustmentId);
        Task AddAsync(StockAdjustment stockAdjustment);
        Task UpdateAsync(StockAdjustment stockAdjustment);
        Task DeleteAsync(Guid stockAdjustmentId);
        Task<IEnumerable<StockAdjustment>> GetByProductBatchIdAsync(Guid productBatchId);
        Task<IEnumerable<StockAdjustment>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<StockAdjustment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<StockAdjustment>> GetPendingApprovalsAsync();
    }
}