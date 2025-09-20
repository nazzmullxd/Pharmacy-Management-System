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
    public class SalesService : ISalesService
    {
        private readonly ISaleRepository _saleRepository;
        private readonly ISaleItemRepository _saleItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;

        public SalesService(
            ISaleRepository saleRepository,
            ISaleItemRepository saleItemRepository,
            IProductRepository productRepository,
            ICustomerRepository customerRepository,
            IUserRepository userRepository)
        {
            _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
            _saleItemRepository = saleItemRepository ?? throw new ArgumentNullException(nameof(saleItemRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<IEnumerable<SaleDTO>> GetAllSalesAsync()
        {
            var sales = await _saleRepository.GetAllAsync();
            var result = new List<SaleDTO>();
            foreach (var sale in sales)
                result.Add(await MapToDTO(sale));
            return result;
        }

        public async Task<SaleDTO?> GetSaleByIdAsync(Guid saleId)
        {
            var sale = await _saleRepository.GetByIdAsync(saleId);
            return sale != null ? await MapToDTO(sale) : null;
        }

        public async Task<SaleDTO> CreateSaleAsync(SaleDTO saleDto)
        {
            if (saleDto == null) throw new ArgumentNullException(nameof(saleDto));
            
            // Comprehensive validation
            await ValidateSale(saleDto);

            // Additional business rule validations
            if (!await ValidateStockAvailability(saleDto))
                throw new InvalidOperationException("Insufficient stock for one or more items in the sale");

            // Recompute item totals and overall total
            RecalculateSaleTotals(saleDto);

            var sale = MapToEntity(saleDto);
            sale.SaleID = Guid.NewGuid();
            sale.SaleDate = DateTime.UtcNow; // Always store UTC

            await _saleRepository.AddAsync(sale);

            foreach (var itemDto in saleDto.SaleItems)
            {
                var saleItem = MapToSaleItemEntity(itemDto, sale.SaleID);
                await _saleItemRepository.AddAsync(saleItem);

                // Update product stock
                var product = await _productRepository.GetByIdAsync(itemDto.ProductID);
                if (product != null)
                {
                    product.TotalStock -= itemDto.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
            }

            // Persist recomputed total (source of truth)
            sale.TotalAmount = saleDto.TotalAmount;
            await _saleRepository.UpdateAsync(sale);

            return await MapToDTO(sale);
        }

        public async Task<SaleDTO> UpdateSaleAsync(SaleDTO saleDto)
        {
            if (saleDto == null) throw new ArgumentNullException(nameof(saleDto));

            var existingSale = await _saleRepository.GetByIdAsync(saleDto.SaleID);
            if (existingSale == null)
                throw new ArgumentException("Sale not found.", nameof(saleDto.SaleID));

            await ValidateSale(saleDto);
            
            // Additional validation for stock availability (considering existing sale items)
            if (!await ValidateStockAvailabilityForUpdate(saleDto, existingSale.SaleID))
                throw new InvalidOperationException("Insufficient stock for updated sale items");

            RecalculateSaleTotals(saleDto);

            var updatedEntity = MapToEntity(saleDto);
            // Preserve original date
            updatedEntity.SaleDate = existingSale.SaleDate;

            // Handle stock adjustments - restore stock from existing items first
            var existingItems = await _saleItemRepository.GetBySaleIdAsync(updatedEntity.SaleID);
            foreach (var existingItem in existingItems)
            {
                var product = await _productRepository.GetByIdAsync(existingItem.ProductID);
                if (product != null)
                {
                    product.TotalStock += existingItem.Quantity; // Restore stock
                    await _productRepository.UpdateAsync(product);
                }
                await _saleItemRepository.DeleteAsync(existingItem.SaleItemID);
            }

            // Add new items and adjust stock
            foreach (var itemDto in saleDto.SaleItems)
            {
                var newItem = MapToSaleItemEntity(itemDto, updatedEntity.SaleID);
                await _saleItemRepository.AddAsync(newItem);
                
                var product = await _productRepository.GetByIdAsync(itemDto.ProductID);
                if (product != null)
                {
                    product.TotalStock -= itemDto.Quantity; // Deduct new quantity
                    await _productRepository.UpdateAsync(product);
                }
            }

            updatedEntity.TotalAmount = saleDto.TotalAmount;
            await _saleRepository.UpdateAsync(updatedEntity);

            return await MapToDTO(updatedEntity);
        }

        public async Task<bool> DeleteSaleAsync(Guid saleId)
        {
            try
            {
                var sale = await _saleRepository.GetByIdAsync(saleId);
                if (sale == null)
                    return false;

                // Get sale items and restore stock
                var saleItems = await _saleItemRepository.GetBySaleIdAsync(saleId);
                foreach (var item in saleItems)
                {
                    // Restore stock to products
                    var product = await _productRepository.GetByIdAsync(item.ProductID);
                    if (product != null)
                    {
                        product.TotalStock += item.Quantity;
                        await _productRepository.UpdateAsync(product);
                    }
                    
                    await _saleItemRepository.DeleteAsync(item.SaleItemID);
                }

                await _saleRepository.DeleteAsync(saleId);
                return true;
            }
            catch (Exception)
            {
                return false; // Logging can be added if repository doesn't already log.
            }
        }

        public async Task<IEnumerable<SaleDTO>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var sales = await _saleRepository.GetByDateRangeAsync(startDate, endDate);
            var list = new List<SaleDTO>();
            foreach (var sale in sales)
                list.Add(await MapToDTO(sale));
            return list;
        }

        public async Task<IEnumerable<SaleDTO>> GetSalesByCustomerAsync(Guid customerId)
        {
            var sales = await _saleRepository.GetByCustomerIdAsync(customerId);
            var list = new List<SaleDTO>();
            foreach (var sale in sales)
                list.Add(await MapToDTO(sale));
            return list;
        }

        public async Task<IEnumerable<SaleDTO>> GetSalesByUserAsync(Guid userId)
        {
            var sales = await _saleRepository.GetByUserIdAsync(userId);
            var list = new List<SaleDTO>();
            foreach (var sale in sales)
                list.Add(await MapToDTO(sale));
            return list;
        }

        public async Task<decimal> GetTotalSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var sales = await _saleRepository.GetByDateRangeAsync(startDate, endDate);
            return sales.Sum(s => s.TotalAmount);
        }

        public async Task<IEnumerable<TopProductDTO>> GetTopSellingProductsAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null)
        {
            IEnumerable<Sale> sales;

            if (startDate.HasValue && endDate.HasValue)
            {
                sales = await _saleRepository.GetByDateRangeAsync(startDate.Value, endDate.Value);
            }
            else
            {
                sales = await _saleRepository.GetAllAsync(); // Fixed: added await and parentheses
            }

            // Cache products to reduce repeated repository calls (in-memory)
            var productCache = new Dictionary<Guid, (string Name, string Category)>();

            var productSales = new Dictionary<Guid, (int Qty, decimal Revenue)>();

            foreach (var sale in sales)
            {
                var items = await _saleItemRepository.GetBySaleIdAsync(sale.SaleID);
                foreach (var item in items)
                {
                    if (!productCache.TryGetValue(item.ProductID, out var prodInfo))
                    {
                        var prod = await _productRepository.GetByIdAsync(item.ProductID);
                        prodInfo = (prod?.ProductName ?? string.Empty, prod?.Category ?? string.Empty);
                        productCache[item.ProductID] = prodInfo;
                    }

                    var lineRevenue = (item.UnitPrice * item.Quantity) - item.Discount;
                    if (productSales.TryGetValue(item.ProductID, out var agg))
                        productSales[item.ProductID] = (agg.Qty + item.Quantity, agg.Revenue + lineRevenue);
                    else
                        productSales[item.ProductID] = (item.Quantity, lineRevenue);
                }
            }

            return productSales
                .OrderByDescending(p => p.Value.Qty)
                .Take(count)
                .Select((kv, i) => new TopProductDTO
                {
                    ProductID = kv.Key,
                    ProductName = productCache[kv.Key].Name,
                    Category = productCache[kv.Key].Category,
                    TotalQuantitySold = kv.Value.Qty,
                    TotalRevenue = kv.Value.Revenue,
                    Rank = i + 1
                });
        }

        public async Task<bool> ProcessSaleAsync(SaleDTO saleDto)
        {
            try
            {
                await CreateSaleAsync(saleDto);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(Guid saleId, string paymentStatus)
        {
            // Validate against known statuses (non-security)
            if (string.IsNullOrWhiteSpace(paymentStatus))
                return false;

            var normalized = paymentStatus.Trim();

            var sale = await _saleRepository.GetByIdAsync(saleId);
            if (sale == null) return false;

            sale.PaymentStatus = normalized;
            await _saleRepository.UpdateAsync(sale);
            return true;
        }

        // Additional validation and utility methods implementation
        public async Task<bool> ValidateProductStockAsync(Guid productId, int requestedQuantity)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(productId);
                return product != null && product.IsActive && product.TotalStock >= requestedQuantity;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CanProcessSaleAsync(SaleDTO saleDto)
        {
            try
            {
                if (saleDto?.SaleItems == null || !saleDto.SaleItems.Any())
                    return false;

                // Check if all products exist and have sufficient stock
                foreach (var item in saleDto.SaleItems)
                {
                    if (!await ValidateProductStockAsync(item.ProductID, item.Quantity))
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Dictionary<Guid, int>> GetAvailableStockAsync(IEnumerable<Guid> productIds)
        {
            var stockDictionary = new Dictionary<Guid, int>();
            
            try
            {
                foreach (var productId in productIds)
                {
                    var product = await _productRepository.GetByIdAsync(productId);
                    stockDictionary[productId] = product?.TotalStock ?? 0;
                }
            }
            catch
            {
                // Return empty dictionary on error
            }

            return stockDictionary;
        }

        public async Task<decimal> CalculateSaleTotalAsync(SaleDTO saleDto)
        {
            try
            {
                if (saleDto?.SaleItems == null)
                    return 0m;

                decimal total = 0m;
                foreach (var item in saleDto.SaleItems)
                {
                    // Validate product exists and price is reasonable
                    var product = await _productRepository.GetByIdAsync(item.ProductID);
                    if (product != null)
                    {
                        var lineTotal = (item.UnitPrice * item.Quantity) - item.Discount;
                        total += Math.Max(0, lineTotal); // Prevent negative line totals
                    }
                }

                return total;
            }
            catch
            {
                return 0m;
            }
        }

        public async Task<bool> IsSaleValidAsync(SaleDTO saleDto)
        {
            try
            {
                await ValidateSale(saleDto);
                return await CanProcessSaleAsync(saleDto);
            }
            catch
            {
                return false;
            }
        }

        private async Task ValidateSale(SaleDTO saleDto)
        {
            if (saleDto == null)
                throw new ArgumentNullException(nameof(saleDto));

            // Validate customer (allow empty for walk-in customers)
            if (saleDto.CustomerID != Guid.Empty)
            {
                var customer = await _customerRepository.GetByIdAsync(saleDto.CustomerID);
                if (customer == null)
                    throw new ArgumentException("Invalid customer ID - customer not found", nameof(saleDto.CustomerID));
            }

            // Validate user
            if (saleDto.UserID == Guid.Empty)
                throw new ArgumentException("User ID is required", nameof(saleDto.UserID));

            var user = await _userRepository.GetByIdAsync(saleDto.UserID);
            if (user == null)
                throw new ArgumentException("Invalid user ID - user not found", nameof(saleDto.UserID));

            // Validate sale items
            if (saleDto.SaleItems == null || saleDto.SaleItems.Count == 0)
                throw new ArgumentException("Sale must contain at least one item", nameof(saleDto.SaleItems));

            // Validate payment status
            var validPaymentStatuses = new[] { "Paid", "Pending", "Partial", "Cancelled" };
            if (!validPaymentStatuses.Contains(saleDto.PaymentStatus))
                throw new ArgumentException($"Invalid payment status. Must be one of: {string.Join(", ", validPaymentStatuses)}", nameof(saleDto.PaymentStatus));

            // Validate sale date
            if (saleDto.SaleDate > DateTime.UtcNow.AddDays(1))
                throw new ArgumentException("Sale date cannot be in the future", nameof(saleDto.SaleDate));

            // Validate each sale item
            foreach (var item in saleDto.SaleItems)
            {
                await ValidateSaleItem(item);
            }

            // Validate total amount consistency
            var calculatedTotal = saleDto.SaleItems.Sum(i => (i.UnitPrice * i.Quantity) - i.Discount);
            if (Math.Abs(saleDto.TotalAmount - calculatedTotal) > 0.01m)
                throw new ArgumentException($"Total amount mismatch. Expected: {calculatedTotal:C}, Provided: {saleDto.TotalAmount:C}", nameof(saleDto.TotalAmount));
        }

        private async Task ValidateSaleItem(SaleItemDTO item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            // Validate product exists
            if (item.ProductID == Guid.Empty)
                throw new ArgumentException("Product ID is required for all sale items", nameof(item.ProductID));

            var product = await _productRepository.GetByIdAsync(item.ProductID);
            if (product == null)
                throw new ArgumentException($"Product not found: {item.ProductID}", nameof(item.ProductID));

            if (!product.IsActive)
                throw new ArgumentException($"Product is not active: {product.ProductName}", nameof(item.ProductID));

            // Validate quantity
            if (item.Quantity <= 0)
                throw new ArgumentException("Item quantity must be greater than zero", nameof(item.Quantity));

            // Validate stock availability
            if (product.TotalStock < item.Quantity)
                throw new ArgumentException($"Insufficient stock for {product.ProductName}. Available: {product.TotalStock}, Requested: {item.Quantity}", nameof(item.Quantity));

            // Validate unit price
            if (item.UnitPrice <= 0)
                throw new ArgumentException("Item unit price must be greater than zero", nameof(item.UnitPrice));

            // Validate discount
            if (item.Discount < 0)
                throw new ArgumentException("Item discount cannot be negative", nameof(item.Discount));

            var lineTotal = item.UnitPrice * item.Quantity;
            if (item.Discount > lineTotal)
                throw new ArgumentException($"Item discount cannot exceed line value. Line total: {lineTotal:C}, Discount: {item.Discount:C}", nameof(item.Discount));

            // Validate unit price is reasonable (not significantly different from product default)
            var priceVariance = Math.Abs(item.UnitPrice - product.UnitPrice) / product.UnitPrice;
            if (priceVariance > 0.5m) // Allow 50% variance
                throw new ArgumentException($"Unit price variance too high for {product.ProductName}. Product price: {product.UnitPrice:C}, Sale price: {item.UnitPrice:C}", nameof(item.UnitPrice));
        }

        private async Task<bool> ValidateStockAvailability(SaleDTO saleDto)
        {
            if (saleDto?.SaleItems == null)
                return false;

            // Group items by product to handle multiple line items for same product
            var productQuantities = saleDto.SaleItems
                .GroupBy(i => i.ProductID)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            foreach (var productQuantity in productQuantities)
            {
                var product = await _productRepository.GetByIdAsync(productQuantity.Key);
                if (product == null || product.TotalStock < productQuantity.Value)
                    return false;
            }

            return true;
        }

        private async Task<bool> ValidateStockAvailabilityForUpdate(SaleDTO saleDto, Guid existingSaleId)
        {
            if (saleDto?.SaleItems == null)
                return false;

            // Get existing sale items to calculate stock differences
            var existingItems = await _saleItemRepository.GetBySaleIdAsync(existingSaleId);
            var existingQuantities = existingItems
                .GroupBy(i => i.ProductID)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            // Group new items by product
            var newQuantities = saleDto.SaleItems
                .GroupBy(i => i.ProductID)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            // Check stock for each product considering the differences
            var allProductIds = existingQuantities.Keys.Union(newQuantities.Keys);
            
            foreach (var productId in allProductIds)
            {
                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                    return false;

                var existingQty = existingQuantities.GetValueOrDefault(productId, 0);
                var newQty = newQuantities.GetValueOrDefault(productId, 0);
                var netChange = newQty - existingQty;

                // If net change is positive (more stock needed), check availability
                if (netChange > 0 && product.TotalStock < netChange)
                    return false;
            }

            return true;
        }

        private static void RecalculateSaleTotals(SaleDTO saleDto)
        {
            decimal total = 0;
            foreach (var item in saleDto.SaleItems)
            {
                item.RecomputeTotal();
                total += item.TotalPrice;
            }
            saleDto.TotalAmount = total;
        }

        private async Task<SaleDTO> MapToDTO(Sale sale)
        {
            var customer = await _customerRepository.GetByIdAsync(sale.CustomerID);
            var user = await _userRepository.GetByIdAsync(sale.UserID);
            var saleItems = await _saleItemRepository.GetBySaleIdAsync(sale.SaleID);

            var saleItemDtos = new List<SaleItemDTO>();
            foreach (var item in saleItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductID);
                var dto = new SaleItemDTO
                {
                    SaleItemID = item.SaleItemID,
                    SaleID = item.SaleID,
                    ProductID = item.ProductID,
                    ProductBatchID = item.ProductBatchID,
                    ProductName = product?.ProductName ?? string.Empty,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Discount = item.Discount,
                    BatchNumber = item.BatchNumber
                };
                dto.RecomputeTotal();
                saleItemDtos.Add(dto);
            }

            // Ensure total matches recomputed items (defensive normalization)
            var recomputed = saleItemDtos.Sum(i => i.TotalPrice);

            return new SaleDTO
            {
                SaleID = sale.SaleID,
                CustomerID = sale.CustomerID,
                CustomerName = customer?.CustomerName ?? string.Empty,
                UserID = sale.UserID,
                UserName = user != null ? $"{user.FirstName} {user.LastName}" : string.Empty,
                SaleDate = sale.SaleDate,
                TotalAmount = recomputed, // override with authoritative aggregation
                PaymentStatus = sale.PaymentStatus,
                Note = sale.Note,
                SaleItems = saleItemDtos
            };
        }

        private static Sale MapToEntity(SaleDTO saleDto)
        {
            return new Sale
            {
                SaleID = saleDto.SaleID,
                CustomerID = saleDto.CustomerID,
                UserID = saleDto.UserID,
                SaleDate = saleDto.SaleDate,
                TotalAmount = saleDto.TotalAmount,
                PaymentStatus = saleDto.PaymentStatus,
                Note = saleDto.Note
            };
        }

        private static SaleItem MapToSaleItemEntity(SaleItemDTO itemDto, Guid saleId)
        {
            return new SaleItem
            {
                SaleItemID = itemDto.SaleItemID == Guid.Empty ? Guid.NewGuid() : itemDto.SaleItemID,
                SaleID = saleId,
                ProductID = itemDto.ProductID,
                ProductBatchID = itemDto.ProductBatchID,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                Discount = itemDto.Discount,
                BatchNumber = itemDto.BatchNumber
            };
        }
    }
}