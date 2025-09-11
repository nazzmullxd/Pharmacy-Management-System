// Stock Adjustment Service Implementation
using Business.DTO;
using Business.Interfaces;
using Database.Interfaces;
using Database.Model;

namespace Business.Services
{
    public class StockAdjustmentService : IStockAdjustmentService
    {
        private readonly IProductBatchRepository _productBatchRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public StockAdjustmentService(
            IProductBatchRepository productBatchRepository,
            IAuditLogRepository auditLogRepository)
        {
            _productBatchRepository = productBatchRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<IEnumerable<StockAdjustmentDTO>> GetAllStockAdjustmentsAsync()
        {
            // No StockAdjustment table exists; return empty collection.
            return new List<StockAdjustmentDTO>();
        }

        public async Task<StockAdjustmentDTO?> GetStockAdjustmentByIdAsync(Guid adjustmentId)
        {
            // No persistent adjustments to retrieve.
            return null;
        }

        public async Task<StockAdjustmentDTO> CreateStockAdjustmentAsync(StockAdjustmentDTO adjustmentDto)
        {
            if (adjustmentDto == null)
                throw new ArgumentNullException(nameof(adjustmentDto));

            // Process immediately since there is no pending storage.
            var processed = await ProcessStockAdjustmentAsync(adjustmentDto);
            if (!processed)
                throw new InvalidOperationException("Failed to process stock adjustment");

            return adjustmentDto;
        }

        public async Task<StockAdjustmentDTO> UpdateStockAdjustmentAsync(StockAdjustmentDTO adjustmentDto)
        {
            if (adjustmentDto == null)
                throw new ArgumentNullException(nameof(adjustmentDto));
            // No persistence layer to update; re-process as correction.
            var processed = await ProcessStockAdjustmentAsync(adjustmentDto);
            if (!processed)
                throw new InvalidOperationException("Failed to process stock adjustment update");
            return adjustmentDto;
        }

        public async Task<bool> DeleteStockAdjustmentAsync(Guid adjustmentId)
        {
            // Nothing to delete without persistence.
            return false;
        }

        public async Task<IEnumerable<StockAdjustmentDTO>> GetAdjustmentsByProductAsync(Guid productId)
        {
            return new List<StockAdjustmentDTO>();
        }

        public async Task<IEnumerable<StockAdjustmentDTO>> GetAdjustmentsByUserAsync(Guid userId)
        {
            return new List<StockAdjustmentDTO>();
        }

        public async Task<IEnumerable<StockAdjustmentDTO>> GetAdjustmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return new List<StockAdjustmentDTO>();
        }

        public async Task<bool> ApproveStockAdjustmentAsync(Guid adjustmentId, Guid approvedBy)
        {
            // Without persisted adjustment, simply create an audit log entry.
            await LogAudit("APPROVE", "StockAdjustment", adjustmentId, $"Approved by {approvedBy}");
            return true;
        }

        public async Task<bool> RejectStockAdjustmentAsync(Guid adjustmentId, string reason)
        {
            await LogAudit("REJECT", "StockAdjustment", adjustmentId, $"Reason: {reason}");
            return true;
        }

        public async Task<IEnumerable<StockAdjustmentDTO>> GetPendingApprovalsAsync()
        {
            return new List<StockAdjustmentDTO>();
        }

        public async Task<bool> ProcessStockAdjustmentAsync(StockAdjustmentDTO adjustmentDto)
        {
            if (adjustmentDto == null)
                throw new ArgumentNullException(nameof(adjustmentDto));

            var batch = await _productBatchRepository.GetByIdAsync(adjustmentDto.ProductBatchID);
            if (batch == null)
                return false;

            var previousQuantity = batch.QuantityInStock;

            // Determine new quantity based on adjustment type
            int newQuantity = previousQuantity;
            if (string.Equals(adjustmentDto.AdjustmentType, "Increase", StringComparison.OrdinalIgnoreCase))
            {
                newQuantity = previousQuantity + adjustmentDto.AdjustedQuantity;
            }
            else if (string.Equals(adjustmentDto.AdjustmentType, "Decrease", StringComparison.OrdinalIgnoreCase))
            {
                newQuantity = previousQuantity - adjustmentDto.AdjustedQuantity;
            }
            else // Correction sets absolute quantity
            {
                newQuantity = adjustmentDto.AdjustedQuantity;
            }

            if (newQuantity < 0)
                return false;

            batch.QuantityInStock = newQuantity;
            await _productBatchRepository.UpdateAsync(batch);

            // Fill DTO summary
            adjustmentDto.PreviousQuantity = previousQuantity;
            adjustmentDto.QuantityDifference = newQuantity - previousQuantity;
            adjustmentDto.AdjustmentDate = DateTime.UtcNow;

            var details = $"Batch {batch.ProductBatchID} adjusted from {previousQuantity} to {newQuantity}. Reason: {adjustmentDto.Reason}";
            await LogAudit("ADJUST", "ProductBatch", batch.ProductBatchID, details);
            return true;
        }

        private async Task LogAudit(string action, string entityType, Guid entityId, string details)
        {
            // Persist audit via repository when available
            var audit = new AuditLog
            {
                AuditLogID = Guid.NewGuid(),
                UserID = adjustmentUserIdFallback, // placeholder if caller does not provide
                Action = action,
                Details = $"{entityType}:{entityId} - {details}",
                IPAddress = string.Empty,
                ActionDate = DateTime.UtcNow
            };

            try
            {
                await _auditLogRepository.AddAsync(audit);
            }
            catch
            {
                // Swallow to avoid breaking business flow if audit repo is unavailable
            }
        }

        // In absence of user context pipeline here, use a static fallback.
        private static readonly Guid adjustmentUserIdFallback = Guid.Empty;
    }
}
