using Business.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Interfaces
{
    public interface IStockAdjustmentService
    {
        Task<IEnumerable<StockAdjustmentDTO>> GetAllStockAdjustmentsAsync();
        Task<StockAdjustmentDTO?> GetStockAdjustmentByIdAsync(Guid adjustmentId);
        Task<StockAdjustmentResultDTO> CreateStockAdjustmentAsync(StockAdjustmentDTO adjustmentDto);
        Task<StockAdjustmentDTO> UpdateStockAdjustmentAsync(StockAdjustmentDTO adjustmentDto);
        Task<bool> DeleteStockAdjustmentAsync(Guid adjustmentId);
        Task<IEnumerable<StockAdjustmentDTO>> GetAdjustmentsByProductAsync(Guid productId);
        Task<IEnumerable<StockAdjustmentDTO>> GetAdjustmentsByUserAsync(Guid userId);
        Task<IEnumerable<StockAdjustmentDTO>> GetAdjustmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<bool> ApproveStockAdjustmentAsync(Guid adjustmentId, Guid approvedBy);
        Task<bool> RejectStockAdjustmentAsync(Guid adjustmentId, string reason);
        Task<IEnumerable<StockAdjustmentDTO>> GetPendingApprovalsAsync();
    }
}