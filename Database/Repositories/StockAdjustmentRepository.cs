using Database.Context;
using Database.Interfaces;
using Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class StockAdjustmentRepository : IStockAdjustmentRepository
    {
        private readonly PharmacyManagementContext _context;

        public StockAdjustmentRepository(PharmacyManagementContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "Context cannot be null");
        }

        public async Task<IEnumerable<StockAdjustment>> GetAllAsync()
        {
            return await _context.StockAdjustments.ToListAsync();
        }

        public async Task<StockAdjustment?> GetByIdAsync(Guid stockAdjustmentId)
        {
            return await _context.StockAdjustments
                .FirstOrDefaultAsync(sa => sa.StockAdjustmentID == stockAdjustmentId);
        }

        public async Task AddAsync(StockAdjustment stockAdjustment)
        {
            if (stockAdjustment == null)
            {
                throw new ArgumentNullException(nameof(stockAdjustment), "StockAdjustment cannot be null");
            }

            await _context.StockAdjustments.AddAsync(stockAdjustment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(StockAdjustment stockAdjustment)
        {
            if (stockAdjustment == null)
            {
                throw new ArgumentNullException(nameof(stockAdjustment), "StockAdjustment cannot be null");
            }

            var existingStockAdjustment = await _context.StockAdjustments
                .FirstOrDefaultAsync(sa => sa.StockAdjustmentID == stockAdjustment.StockAdjustmentID);

            if (existingStockAdjustment == null)
            {
                throw new InvalidOperationException("StockAdjustment not found");
            }

            _context.Entry(existingStockAdjustment).CurrentValues.SetValues(stockAdjustment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid stockAdjustmentId)
        {
            var stockAdjustment = await _context.StockAdjustments
                .FirstOrDefaultAsync(sa => sa.StockAdjustmentID == stockAdjustmentId);

            if (stockAdjustment == null)
            {
                throw new InvalidOperationException("StockAdjustment not found");
            }

            _context.StockAdjustments.Remove(stockAdjustment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<StockAdjustment>> GetByProductBatchIdAsync(Guid productBatchId)
        {
            return await _context.StockAdjustments
                .Where(sa => sa.ProductBatchID == productBatchId)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockAdjustment>> GetByUserIdAsync(Guid userId)
        {
            return await _context.StockAdjustments
                .Where(sa => sa.UserID == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockAdjustment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.StockAdjustments
                .Where(sa => sa.AdjustmentDate >= startDate && sa.AdjustmentDate <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockAdjustment>> GetPendingApprovalsAsync()
        {
            return await _context.StockAdjustments
                .Where(sa => !sa.IsApproved)
                .ToListAsync();
        }
    }
}