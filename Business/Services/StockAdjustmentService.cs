using Business.DTO;
using Business.Interfaces;
using Database.Interfaces;
using Database.Model;
using Microsoft.Extensions.Logging;

namespace Business.Services
{
    public class StockAdjustmentService : IStockAdjustmentService
    {
        private readonly IProductBatchRepository _productBatchRepository;
        private readonly IStockAdjustmentRepository _stockAdjustmentRepository;
        private readonly ILogger<StockAdjustmentService> _logger;

        public StockAdjustmentService(
            IProductBatchRepository productBatchRepository,
            IStockAdjustmentRepository stockAdjustmentRepository,
            ILogger<StockAdjustmentService> logger)
        {
            _productBatchRepository = productBatchRepository ?? throw new ArgumentNullException(nameof(productBatchRepository));
            _stockAdjustmentRepository = stockAdjustmentRepository ?? throw new ArgumentNullException(nameof(stockAdjustmentRepository));
            _logger = logger;
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

        public async Task<StockAdjustmentResultDTO> CreateStockAdjustmentAsync(StockAdjustmentDTO adjustmentDto)
        {
            if (adjustmentDto == null)
                return new StockAdjustmentResultDTO { Success = false, ErrorMessage = "Adjustment data is required." };

            try
            {
                ValidateAdjustment(adjustmentDto);

                var batch = await _productBatchRepository.GetByIdAsync(adjustmentDto.ProductBatchID);
                if (batch == null)
                    return new StockAdjustmentResultDTO { Success = false, ErrorMessage = "Product batch not found." };

                if (batch.ExpiryDate <= DateTime.UtcNow)
                    return new StockAdjustmentResultDTO { Success = false, ErrorMessage = "Cannot adjust expired batch." };

                var previousQuantity = batch.QuantityInStock;
                int newQuantity = adjustmentDto.AdjustmentType.ToLower() switch
                {
                    "increase" => previousQuantity + adjustmentDto.AdjustedQuantity,
                    "decrease" => previousQuantity - adjustmentDto.AdjustedQuantity,
                    "correction" => adjustmentDto.AdjustedQuantity,
                    _ => throw new InvalidOperationException("Invalid adjustment type")
                };

                if (newQuantity < 0)
                    return new StockAdjustmentResultDTO { Success = false, ErrorMessage = "Resulting stock would be negative." };

                batch.QuantityInStock = newQuantity;
                await _productBatchRepository.UpdateAsync(batch);

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

                _logger.LogInformation("Stock adjustment created: {AdjustmentId} for batch {BatchId} by user {UserId}. Type: {Type}, Reason: {Reason}",
                    adjustment.StockAdjustmentID, batch.ProductBatchID, adjustmentDto.UserID, adjustmentDto.AdjustmentType, adjustmentDto.Reason);

                return new StockAdjustmentResultDTO { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create stock adjustment for batch {BatchId}", adjustmentDto.ProductBatchID);
                return new StockAdjustmentResultDTO { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<StockAdjustmentDTO> UpdateStockAdjustmentAsync(StockAdjustmentDTO adjustmentDto)
        {
            if (adjustmentDto == null)
                throw new ArgumentNullException(nameof(adjustmentDto));

            var adjustment = await _stockAdjustmentRepository.GetByIdAsync(adjustmentDto.StockAdjustmentID);
            if (adjustment == null)
                throw new InvalidOperationException("Stock adjustment not found");

            adjustment.AdjustedQuantity = adjustmentDto.AdjustedQuantity;
            adjustment.Reason = adjustmentDto.Reason;
            adjustment.AdjustmentType = adjustmentDto.AdjustmentType;
            adjustment.AdjustmentDate = DateTime.UtcNow;

            await _stockAdjustmentRepository.UpdateAsync(adjustment);

            _logger.LogInformation("Stock adjustment updated: {AdjustmentId}", adjustment.StockAdjustmentID);

            return MapToDTO(adjustment);
        }

        public async Task<bool> DeleteStockAdjustmentAsync(Guid adjustmentId)
        {
            var adjustment = await _stockAdjustmentRepository.GetByIdAsync(adjustmentId);
            if (adjustment == null)
                return false;

            await _stockAdjustmentRepository.DeleteAsync(adjustmentId);

            _logger.LogInformation("Stock adjustment deleted: {AdjustmentId}", adjustmentId);
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

        public async Task<bool> ApproveStockAdjustmentAsync(Guid adjustmentId, Guid approvedBy)
        {
            var adjustment = await _stockAdjustmentRepository.GetByIdAsync(adjustmentId);
            if (adjustment == null)
                return false;

            adjustment.IsApproved = true;
            adjustment.ApprovedBy = approvedBy;
            adjustment.ApprovalDate = DateTime.UtcNow;

            await _stockAdjustmentRepository.UpdateAsync(adjustment);

            _logger.LogInformation("Stock adjustment approved: {AdjustmentId} by {UserId}", adjustmentId, approvedBy);
            return true;
        }

        public async Task<bool> RejectStockAdjustmentAsync(Guid adjustmentId, string reason)
        {
            var adjustment = await _stockAdjustmentRepository.GetByIdAsync(adjustmentId);
            if (adjustment == null)
                return false;

            adjustment.IsApproved = false;
            adjustment.Reason += $" (Rejected: {reason})";
            await _stockAdjustmentRepository.UpdateAsync(adjustment);

            _logger.LogInformation("Stock adjustment rejected: {AdjustmentId}. Reason: {Reason}", adjustmentId, reason);
            return true;
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