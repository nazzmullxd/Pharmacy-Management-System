using Business.DTO;
using Business.Interfaces;
using Database.Interfaces;
using Database.Model;

namespace Business.Services
{
    public class StockAdjustmentService : IStockAdjustmentService
    {
        private readonly IProductBatchRepository _productBatchRepository;
        private readonly IStockAdjustmentRepository _stockAdjustmentRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public StockAdjustmentService(
            IProductBatchRepository productBatchRepository,
            IStockAdjustmentRepository stockAdjustmentRepository,
            IAuditLogRepository auditLogRepository)
        {
            _productBatchRepository = productBatchRepository ?? throw new ArgumentNullException(nameof(productBatchRepository));
            _stockAdjustmentRepository = stockAdjustmentRepository ?? throw new ArgumentNullException(nameof(stockAdjustmentRepository));
            _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
        }

        public async Task<IEnumerable<StockAdjustmentDTO>> GetAllStockAdjustmentsAsync()
        {
            var adjustments = await _stockAdjustmentRepository.GetAllAsync();
            return adjustments.Select(MapToDTO);
        }

        public async Task<StockAdjustmentDTO?> GetStockAdjustmentByIdAsync(Guid adjustmentId)
        {
            var adjustment = await _stockAdjustmentRepository.GetByIdAsync(adjustmentId);
            return adjustment != null ? MapToDTO(adjustment) : null;
        }

        public async Task<StockAdjustmentDTO> CreateStockAdjustmentAsync(StockAdjustmentDTO adjustmentDto)
        {
            if (adjustmentDto == null)
                throw new ArgumentNullException(nameof(adjustmentDto));

            ValidateAdjustment(adjustmentDto);

            var batch = await _productBatchRepository.GetByIdAsync(adjustmentDto.ProductBatchID);
            if (batch == null)
                throw new InvalidOperationException("Product batch not found");

            var previousQuantity = batch.QuantityInStock;

            // Determine new quantity based on adjustment type
            int newQuantity = adjustmentDto.AdjustmentType.ToLower() switch
            {
                "increase" => previousQuantity + adjustmentDto.AdjustedQuantity,
                "decrease" => previousQuantity - adjustmentDto.AdjustedQuantity,
                "correction" => adjustmentDto.AdjustedQuantity,
                _ => throw new InvalidOperationException("Invalid adjustment type")
            };

            if (newQuantity < 0)
                throw new InvalidOperationException("Adjusted quantity cannot result in negative stock");

            // Update product batch
            batch.QuantityInStock = newQuantity;
            await _productBatchRepository.UpdateAsync(batch);

            // Create stock adjustment
            var adjustment = new StockAdjustment
            {
                StockAdjustmentID = Guid.NewGuid(),
                ProductBatchID = adjustmentDto.ProductBatchID,
                PreviousQuantity = previousQuantity,
                AdjustedQuantity = adjustmentDto.AdjustedQuantity,
                QuantityDifference = newQuantity - previousQuantity,
                AdjustmentType = adjustmentDto.AdjustmentType,
                Reason = adjustmentDto.Reason,
                UserID = adjustmentDto.UserID,
                AdjustmentDate = DateTime.UtcNow,
                IsApproved = false
            };

            await _stockAdjustmentRepository.AddAsync(adjustment);

            // Log audit
            var details = $"Batch {batch.ProductBatchID} adjusted from {previousQuantity} to {newQuantity}. Reason: {adjustmentDto.Reason}";
            await LogAudit("ADJUST", "ProductBatch", batch.ProductBatchID, details, adjustmentDto.UserID);

            return MapToDTO(adjustment);
        }

        public async Task<StockAdjustmentDTO> UpdateStockAdjustmentAsync(StockAdjustmentDTO adjustmentDto)
        {
            if (adjustmentDto == null)
                throw new ArgumentNullException(nameof(adjustmentDto));

            ValidateAdjustment(adjustmentDto);

            var adjustment = await _stockAdjustmentRepository.GetByIdAsync(adjustmentDto.StockAdjustmentID);
            if (adjustment == null)
                throw new InvalidOperationException("Stock adjustment not found");

            adjustment.AdjustedQuantity = adjustmentDto.AdjustedQuantity;
            adjustment.Reason = adjustmentDto.Reason;
            adjustment.AdjustmentType = adjustmentDto.AdjustmentType;
            adjustment.AdjustmentDate = DateTime.UtcNow;

            await _stockAdjustmentRepository.UpdateAsync(adjustment);

            // Log audit
            var details = $"Stock adjustment {adjustment.StockAdjustmentID} updated. Reason: {adjustmentDto.Reason}";
            await LogAudit("UPDATE", "StockAdjustment", adjustment.StockAdjustmentID, details, adjustmentDto.UserID);

            return MapToDTO(adjustment);
        }

        public async Task<bool> DeleteStockAdjustmentAsync(Guid adjustmentId)
        {
            var adjustment = await _stockAdjustmentRepository.GetByIdAsync(adjustmentId);
            if (adjustment == null)
                return false;

            await _stockAdjustmentRepository.DeleteAsync(adjustmentId);

            // Log audit
            await LogAudit("DELETE", "StockAdjustment", adjustmentId, "Stock adjustment deleted", adjustment.UserID);
            return true;
        }

        public async Task<IEnumerable<StockAdjustmentDTO>> GetAdjustmentsByProductAsync(Guid productId)
        {
            var adjustments = await _stockAdjustmentRepository.GetByProductBatchIdAsync(productId);
            return adjustments.Select(MapToDTO);
        }

        public async Task<IEnumerable<StockAdjustmentDTO>> GetAdjustmentsByUserAsync(Guid userId)
        {
            var adjustments = await _stockAdjustmentRepository.GetByUserIdAsync(userId);
            return adjustments.Select(MapToDTO);
        }

        public async Task<IEnumerable<StockAdjustmentDTO>> GetAdjustmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var adjustments = await _stockAdjustmentRepository.GetByDateRangeAsync(startDate, endDate);
            return adjustments.Select(MapToDTO);
        }

        public async Task<IEnumerable<StockAdjustmentDTO>> GetPendingApprovalsAsync()
        {
            var adjustments = await _stockAdjustmentRepository.GetPendingApprovalsAsync();
            return adjustments.Select(MapToDTO);
        }

        public async Task<bool> ProcessStockAdjustmentAsync(StockAdjustmentDTO adjustmentDto)
        {
            if (adjustmentDto == null)
                throw new ArgumentNullException(nameof(adjustmentDto));

            var adjustment = await _stockAdjustmentRepository.GetByIdAsync(adjustmentDto.StockAdjustmentID);
            if (adjustment == null)
                return false;

            adjustment.IsApproved = true;
            adjustment.ApprovedBy = adjustmentDto.UserID;
            adjustment.ApprovalDate = DateTime.UtcNow;

            await _stockAdjustmentRepository.UpdateAsync(adjustment);

            // Log audit
            var details = $"Stock adjustment {adjustment.StockAdjustmentID} processed.";
            await LogAudit("PROCESS", "StockAdjustment", adjustment.StockAdjustmentID, details, adjustmentDto.UserID);

            return true;
        }

        public async Task<bool> ApproveStockAdjustmentAsync(Guid adjustmentId, Guid approvedBy)
        {
            var adjustment = await _stockAdjustmentRepository.GetByIdAsync(adjustmentId);
            if (adjustment == null)
                return false;

            adjustment.IsApproved = true;
            adjustment.ApprovedBy = approvedBy;
            adjustment.ApprovalDate = DateTime.UtcNow;

            await _stockAdjustmentRepository.UpdateAsync(adjustment);

            await LogAudit("APPROVE", "StockAdjustment", adjustmentId, $"Approved by {approvedBy}", approvedBy);
            return true;
        }

        public async Task<bool> RejectStockAdjustmentAsync(Guid adjustmentId, string reason)
        {
            var adjustment = await _stockAdjustmentRepository.GetByIdAsync(adjustmentId);
            if (adjustment == null)
                return false;

            await LogAudit("REJECT", "StockAdjustment", adjustmentId, $"Rejected. Reason: {reason}", adjustment.UserID);
            return true;
        }

        private async Task LogAudit(string action, string entityType, Guid entityId, string details, Guid userId)
        {
            var audit = new AuditLog
            {
                AuditLogID = Guid.NewGuid(),
                UserID = userId,
                Action = action,
                Details = $"{entityType}:{entityId} - {details}",
                IPAddress = string.Empty,
                ActionDate = DateTime.UtcNow
            };

            try
            {
                await _auditLogRepository.AddAsync(audit);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log audit: {ex.Message}");
            }
        }

        private void ValidateAdjustment(StockAdjustmentDTO adjustmentDto)
        {
            if (adjustmentDto.AdjustedQuantity <= 0)
                throw new ArgumentException("Adjusted quantity must be greater than zero", nameof(adjustmentDto.AdjustedQuantity));

            if (string.IsNullOrWhiteSpace(adjustmentDto.AdjustmentType))
                throw new ArgumentException("Adjustment type is required", nameof(adjustmentDto.AdjustmentType));

            var validTypes = new[] { "increase", "decrease", "correction" };
            if (!validTypes.Contains(adjustmentDto.AdjustmentType.ToLower()))
                throw new ArgumentException("Invalid adjustment type", nameof(adjustmentDto.AdjustmentType));
        }

        private StockAdjustmentDTO MapToDTO(StockAdjustment adjustment)
        {
            return new StockAdjustmentDTO
            {
                StockAdjustmentID = adjustment.StockAdjustmentID,
                ProductBatchID = adjustment.ProductBatchID,
                PreviousQuantity = adjustment.PreviousQuantity,
                AdjustedQuantity = adjustment.AdjustedQuantity,
                QuantityDifference = adjustment.QuantityDifference,
                AdjustmentType = adjustment.AdjustmentType,
                Reason = adjustment.Reason,
                UserID = adjustment.UserID,
                AdjustmentDate = adjustment.AdjustmentDate,
                IsApproved = adjustment.IsApproved,
                ApprovedBy = adjustment.ApprovedBy,
                ApprovalDate = adjustment.ApprovalDate
            };
        }
    }
}