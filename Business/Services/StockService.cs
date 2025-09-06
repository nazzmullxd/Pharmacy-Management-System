using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.DTO;
using Business.Interfaces;
using Database.Interfaces;
using Database.Model;

namespace Business.Services
{
    public class StockService : IStockService
    {
        private readonly IProductBatchRepository _productBatchRepository;
        private readonly IProductRepository _productRepository;
        private readonly ISupplierRepository _supplierRepository;

        public StockService(
            IProductBatchRepository productBatchRepository,
            IProductRepository productRepository,
            ISupplierRepository supplierRepository)
        {
            _productBatchRepository = productBatchRepository ?? throw new ArgumentNullException(nameof(productBatchRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));
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
            return await MapToDTO(batch);
        }

        public async Task<bool> DeleteProductBatchAsync(Guid batchId)
        {
            try
            {
                await _productBatchRepository.DeleteAsync(batchId);
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

        public async Task<bool> AdjustStockAsync(Guid batchId, int quantityChange, string reason)
        {
            try
            {
                var batch = await _productBatchRepository.GetByIdAsync(batchId);
                if (batch == null)
                    return false;

                var newQuantity = batch.QuantityInStock + quantityChange;
                if (newQuantity < 0)
                    return false; // Cannot have negative stock

                batch.QuantityInStock = newQuantity;
                await _productBatchRepository.UpdateAsync(batch);
                return true;
            }
            catch
            {
                return false;
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

        public async Task<bool> ProcessStockAdjustmentAsync(Guid batchId, int newQuantity, string reason)
        {
            try
            {
                var batch = await _productBatchRepository.GetByIdAsync(batchId);
                if (batch == null)
                    return false;

                if (newQuantity < 0)
                    return false; // Cannot have negative stock

                batch.QuantityInStock = newQuantity;
                await _productBatchRepository.UpdateAsync(batch);
                return true;
            }
            catch
            {
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
    }
}
