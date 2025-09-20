using Business.DTO;
using Business.Interfaces;
using Database.Interfaces;
using Database.Model;
using Microsoft.Extensions.Logging;

namespace Business.Services
{
    public class StockService : IStockService
    {
        private readonly IProductBatchRepository _productBatchRepository;
        private readonly IProductRepository _productRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly ILogger<StockService> _logger;

        public StockService(
            IProductBatchRepository productBatchRepository,
            IProductRepository productRepository,
            ISupplierRepository supplierRepository,
            ILogger<StockService> logger)
        {
            _productBatchRepository = productBatchRepository ?? throw new ArgumentNullException(nameof(productBatchRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));
            _logger = logger;
        }

        public async Task<IEnumerable<ProductBatchDTO>> GetAllProductBatchesAsync()
        {
            var batches = await _productBatchRepository.GetAllAsync();
            var batchDtos = new List<ProductBatchDTO>();

            foreach (var batch in batches)
            {
                var batchDto = await MapToDTO(batch);
                batchDtos.Add(batchDto);
            }

            return batchDtos;
        }

        public async Task<ProductBatchDTO?> GetProductBatchByIdAsync(Guid batchId)
        {
            var batch = await _productBatchRepository.GetByIdAsync(batchId);
            return batch != null ? await MapToDTO(batch) : null;
        }

        public async Task<ProductBatchDTO> CreateProductBatchAsync(ProductBatchDTO batchDto)
        {
            if (batchDto == null)
                throw new ArgumentNullException(nameof(batchDto));

            ValidateProductBatch(batchDto);

            var batch = MapToEntity(batchDto);
            batch.ProductBatchID = Guid.NewGuid();
            batch.CreatedDate = DateTime.UtcNow;

            await _productBatchRepository.AddAsync(batch);

            // Keep product total stock in sync
            await UpdateProductTotalStock(batch.ProductID);

            return await MapToDTO(batch);
        }

        public async Task<ProductBatchDTO> UpdateProductBatchAsync(ProductBatchDTO batchDto)
        {
            if (batchDto == null)
                throw new ArgumentNullException(nameof(batchDto));

            var existingBatch = await _productBatchRepository.GetByIdAsync(batchDto.ProductBatchID);
            if (existingBatch == null)
                throw new ArgumentException("Product batch not found", nameof(batchDto.ProductBatchID));

            ValidateProductBatch(batchDto);

            var batch = MapToEntity(batchDto);
            batch.CreatedDate = existingBatch.CreatedDate; // Preserve original creation date

            await _productBatchRepository.UpdateAsync(batch);

            // Keep product total stock in sync
            await UpdateProductTotalStock(batch.ProductID);

            return await MapToDTO(batch);
        }

        public async Task<bool> DeleteProductBatchAsync(Guid batchId)
        {
            try
            {
                var batch = await _productBatchRepository.GetByIdAsync(batchId);
                if (batch == null)
                    return false;

                await _productBatchRepository.DeleteAsync(batchId);

                // Keep product total stock in sync
                await UpdateProductTotalStock(batch.ProductID);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<ProductBatchDTO>> GetBatchesByProductAsync(Guid productId)
        {
            var batches = await _productBatchRepository.GetByProductIdAsync(productId);
            var batchDtos = new List<ProductBatchDTO>();

            foreach (var batch in batches)
            {
                var batchDto = await MapToDTO(batch);
                batchDtos.Add(batchDto);
            }

            return batchDtos;
        }

        public async Task<IEnumerable<ProductBatchDTO>> GetExpiringBatchesAsync(int daysAhead = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
            var allBatches = await _productBatchRepository.GetAllAsync();
            var expiringBatches = allBatches.Where(b => b.ExpiryDate <= cutoffDate && b.QuantityInStock > 0);

            var batchDtos = new List<ProductBatchDTO>();
            foreach (var batch in expiringBatches)
            {
                var batchDto = await MapToDTO(batch);
                batchDtos.Add(batchDto);
            }

            return batchDtos;
        }

        public async Task<IEnumerable<ProductBatchDTO>> GetExpiredBatchesAsync()
        {
            var allBatches = await _productBatchRepository.GetAllAsync();
            var expiredBatches = allBatches.Where(b => b.ExpiryDate <= DateTime.UtcNow && b.QuantityInStock > 0);

            var batchDtos = new List<ProductBatchDTO>();
            foreach (var batch in expiredBatches)
            {
                var batchDto = await MapToDTO(batch);
                batchDtos.Add(batchDto);
            }

            return batchDtos;
        }

        public async Task<IEnumerable<ExpiryAlertDTO>> GetExpiryAlertsAsync()
        {
            var expiringBatches = await GetExpiringBatchesAsync(30);
            var expiredBatches = await GetExpiredBatchesAsync();
            var alerts = new List<ExpiryAlertDTO>();

            // Add expired batches as critical alerts
            foreach (var batch in expiredBatches)
            {
                alerts.Add(new ExpiryAlertDTO
                {
                    ProductBatchID = batch.ProductBatchID,
                    ProductID = batch.ProductID,
                    ProductName = batch.ProductName,
                    BatchNumber = batch.BatchNumber,
                    ExpiryDate = batch.ExpiryDate,
                    QuantityInStock = batch.QuantityInStock,
                    DaysUntilExpiry = (int)(batch.ExpiryDate - DateTime.UtcNow).TotalDays,
                    AlertLevel = "Critical",
                    SupplierName = batch.SupplierName
                });
            }

            // Add expiring batches as warnings
            foreach (var batch in expiringBatches.Where(b => b.ExpiryDate > DateTime.UtcNow))
            {
                var daysUntilExpiry = (int)(batch.ExpiryDate - DateTime.UtcNow).TotalDays;
                alerts.Add(new ExpiryAlertDTO
                {
                    ProductBatchID = batch.ProductBatchID,
                    ProductID = batch.ProductID,
                    ProductName = batch.ProductName,
                    BatchNumber = batch.BatchNumber,
                    ExpiryDate = batch.ExpiryDate,
                    QuantityInStock = batch.QuantityInStock,
                    DaysUntilExpiry = daysUntilExpiry,
                    AlertLevel = daysUntilExpiry <= 7 ? "Warning" : "Info",
                    SupplierName = batch.SupplierName
                });
            }

            return alerts.OrderBy(a => a.ExpiryDate);
        }

        public async Task<int> GetTotalStockForProductAsync(Guid productId)
        {
            var batches = await _productBatchRepository.GetByProductIdAsync(productId);
            return batches.Where(b => b.ExpiryDate > DateTime.UtcNow).Sum(b => b.QuantityInStock);
        }

        public async Task<Dictionary<Guid, int>> GetTotalStockForMultipleProductsAsync(IEnumerable<Guid> productIds)
        {
            var result = new Dictionary<Guid, int>();
            
            if (!productIds.Any())
                return result;

            // Get all batches for all products in one query
            var allBatches = await _productBatchRepository.GetAllAsync();
            var now = DateTime.UtcNow;
            
            // Group by product and calculate totals
            var stockTotals = allBatches
                .Where(b => productIds.Contains(b.ProductID) && b.ExpiryDate > now)
                .GroupBy(b => b.ProductID)
                .ToDictionary(g => g.Key, g => g.Sum(b => b.QuantityInStock));
            
            // Ensure all requested products are in the result (with 0 if no stock)
            foreach (var productId in productIds)
            {
                result[productId] = stockTotals.TryGetValue(productId, out var stock) ? stock : 0;
            }
            
            return result;
        }

        // Renamed to avoid CS0111 conflict
        public async Task<StockAdjustmentResultDTO> AdjustStockWithResultAsync(Guid batchId, int quantityChange, string reason)
        {
            try
            {
                var batch = await _productBatchRepository.GetByIdAsync(batchId);
                if (batch == null)
                    return new StockAdjustmentResultDTO { Success = false, ErrorMessage = "Batch not found." };

                if (batch.ExpiryDate <= DateTime.UtcNow)
                    return new StockAdjustmentResultDTO { Success = false, ErrorMessage = "Batch is expired." };

                var newQuantity = batch.QuantityInStock + quantityChange;
                if (newQuantity < 0)
                    return new StockAdjustmentResultDTO { Success = false, ErrorMessage = "Resulting stock would be negative." };

                batch.QuantityInStock = newQuantity;
                await _productBatchRepository.UpdateAsync(batch);

                // Keep product total stock in sync
                await UpdateProductTotalStock(batch.ProductID);

                // Log success
                Console.WriteLine($"Stock adjusted for batch {batchId}: {quantityChange} ({reason})");

                return new StockAdjustmentResultDTO { Success = true };
            }
            catch (Exception ex)
            {
                // Log exception
                _logger.LogError(ex, "Stock adjustment failed for batch {BatchId}", batchId);
                return new StockAdjustmentResultDTO { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<IEnumerable<ProductBatchDTO>> GetLowStockBatchesAsync(int threshold = 10)
        {
            var allBatches = await _productBatchRepository.GetAllAsync();
            var lowStockBatches = allBatches.Where(b => b.QuantityInStock <= threshold && b.QuantityInStock > 0);

            var batchDtos = new List<ProductBatchDTO>();
            foreach (var batch in lowStockBatches)
            {
                var batchDto = await MapToDTO(batch);
                batchDtos.Add(batchDto);
            }

            return batchDtos;
        }

        public async Task<bool> AdjustStockAsync(Guid batchId, int quantityChange, string reason)
        {
            try
            {
                var batch = await _productBatchRepository.GetByIdAsync(batchId);
                if (batch == null)
                {
                    _logger.LogWarning("AdjustStockAsync failed: Batch not found. BatchId={BatchId}", batchId);
                    return false;
                }

                if (batch.ExpiryDate <= DateTime.UtcNow)
                {
                    _logger.LogWarning("AdjustStockAsync failed: Batch expired. BatchId={BatchId}, ExpiryDate={ExpiryDate}", batchId, batch.ExpiryDate);
                    return false;
                }

                var newQuantity = batch.QuantityInStock + quantityChange;
                if (newQuantity < 0)
                {
                    _logger.LogWarning("AdjustStockAsync failed: Negative stock. BatchId={BatchId}, CurrentQty={CurrentQty}, Change={Change}", batchId, batch.QuantityInStock, quantityChange);
                    return false;
                }

                batch.QuantityInStock = newQuantity;
                await _productBatchRepository.UpdateAsync(batch);

                // Keep product total stock in sync
                await UpdateProductTotalStock(batch.ProductID);

                _logger.LogInformation("Stock adjusted for batch {BatchId}: Change={Change}, Reason={Reason}, NewQty={NewQty}", batchId, quantityChange, reason, newQuantity);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdjustStockAsync exception for batch {BatchId}", batchId);
                return false;
            }
        }

        public async Task<bool> ProcessStockAdjustmentAsync(Guid batchId, int newQuantity, string reason)
        {
            try
            {
                var batch = await _productBatchRepository.GetByIdAsync(batchId);
                if (batch == null)
                {
                    _logger.LogWarning("ProcessStockAdjustmentAsync failed: Batch not found. BatchId={BatchId}", batchId);
                    return false;
                }

                if (batch.ExpiryDate <= DateTime.UtcNow)
                {
                    _logger.LogWarning("ProcessStockAdjustmentAsync failed: Batch expired. BatchId={BatchId}, ExpiryDate={ExpiryDate}", batchId, batch.ExpiryDate);
                    return false;
                }

                if (newQuantity < 0)
                {
                    _logger.LogWarning("ProcessStockAdjustmentAsync failed: Negative stock. BatchId={BatchId}, NewQty={NewQty}", batchId, newQuantity);
                    return false;
                }

                batch.QuantityInStock = newQuantity;
                await _productBatchRepository.UpdateAsync(batch);

                // Keep product total stock in sync
                await UpdateProductTotalStock(batch.ProductID);

                _logger.LogInformation("Stock adjustment processed for batch {BatchId}: NewQty={NewQty}, Reason={Reason}", batchId, newQuantity, reason);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessStockAdjustmentAsync exception for batch {BatchId}", batchId);
                return false;
            }
        }

        private void ValidateProductBatch(ProductBatchDTO batchDto)
        {
            if (batchDto.ProductID == Guid.Empty)
                throw new ArgumentException("Product ID is required", nameof(batchDto.ProductID));

            if (batchDto.SupplierID == Guid.Empty)
                throw new ArgumentException("Supplier ID is required", nameof(batchDto.SupplierID));

            if (string.IsNullOrWhiteSpace(batchDto.BatchNumber))
                throw new ArgumentException("Batch number is required", nameof(batchDto.BatchNumber));

            if (batchDto.ExpiryDate <= DateTime.UtcNow)
                throw new ArgumentException("Expiry date must be in the future", nameof(batchDto.ExpiryDate));

            if (batchDto.QuantityInStock < 0)
                throw new ArgumentException("Quantity in stock cannot be negative", nameof(batchDto.QuantityInStock));
        }

        private async Task<ProductBatchDTO> MapToDTO(ProductBatch batch)
        {
            var product = await _productRepository.GetByIdAsync(batch.ProductID);
            var supplier = await _supplierRepository.GetByIdAsync(batch.SupplierID);

            return new ProductBatchDTO
            {
                ProductBatchID = batch.ProductBatchID,
                ProductID = batch.ProductID,
                ProductName = product?.ProductName ?? string.Empty,
                SupplierID = batch.SupplierID,
                SupplierName = supplier?.SupplierName ?? string.Empty,
                BatchNumber = batch.BatchNumber,
                ExpiryDate = batch.ExpiryDate,
                QuantityInStock = batch.QuantityInStock,
                CreatedDate = batch.CreatedDate
            };
        }

        private static ProductBatch MapToEntity(ProductBatchDTO batchDto)
        {
            return new ProductBatch
            {
                ProductBatchID = batchDto.ProductBatchID,
                ProductID = batchDto.ProductID,
                SupplierID = batchDto.SupplierID,
                BatchNumber = batchDto.BatchNumber,
                ExpiryDate = batchDto.ExpiryDate,
                QuantityInStock = batchDto.QuantityInStock,
                CreatedDate = batchDto.CreatedDate
            };
        }

        // Helper to update the Product.TotalStock field to reflect current batches
        private async Task UpdateProductTotalStock(Guid productId)
        {
            try
            {
                var total = await GetTotalStockForProductAsync(productId);
                var product = await _productRepository.GetByIdAsync(productId);
                if (product != null)
                {
                    product.TotalStock = total;
                    await _productRepository.UpdateAsync(product);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Product.TotalStock for ProductId={ProductId}", productId);
            }
        }
    }
}