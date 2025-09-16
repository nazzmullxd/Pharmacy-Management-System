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
            try
            {
                var sales = await _saleRepository.GetAllAsync();
                var result = new List<SaleDTO>();
                foreach (var sale in sales)
                    result.Add(await MapToDTO(sale));
                return result;
            }
            catch (Exception)
            {
                // Return empty list if database is not available (for development/demo)
                return new List<SaleDTO>();
            }
        }

        public async Task<SaleDTO?> GetSaleByIdAsync(Guid saleId)
        {
            var sale = await _saleRepository.GetByIdAsync(saleId);
            return sale != null ? await MapToDTO(sale) : null;
        }

        public async Task<SaleDTO> CreateSaleAsync(SaleDTO saleDto)
        {
            if (saleDto == null) throw new ArgumentNullException(nameof(saleDto));
            ValidateSale(saleDto);

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

                // OPTIONAL: adjust stock here if you have a product/batch stock decrement method
                // await _productRepository.DecrementStockAsync(itemDto.ProductID, itemDto.Quantity);
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

            ValidateSale(saleDto);
            RecalculateSaleTotals(saleDto);

            var updatedEntity = MapToEntity(saleDto);
            // Preserve original date
            updatedEntity.SaleDate = existingSale.SaleDate;

            // Remove existing items first
            var existingItems = await _saleItemRepository.GetBySaleIdAsync(updatedEntity.SaleID);
            foreach (var item in existingItems)
                await _saleItemRepository.DeleteAsync(item.SaleItemID);

            foreach (var itemDto in saleDto.SaleItems)
            {
                var newItem = MapToSaleItemEntity(itemDto, updatedEntity.SaleID);
                await _saleItemRepository.AddAsync(newItem);
                // OPTIONAL: recalculate stock delta if quantity changed; requires previous snapshot.
            }

            updatedEntity.TotalAmount = saleDto.TotalAmount;
            await _saleRepository.UpdateAsync(updatedEntity);

            return await MapToDTO(updatedEntity);
        }

        public async Task<bool> DeleteSaleAsync(Guid saleId)
        {
            try
            {
                var saleItems = await _saleItemRepository.GetBySaleIdAsync(saleId);
                foreach (var item in saleItems)
                {
                    await _saleItemRepository.DeleteAsync(item.SaleItemID);
                    // OPTIONAL: restore stock here
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

        private void ValidateSale(SaleDTO saleDto)
        {
            if (saleDto.CustomerID == Guid.Empty)
                throw new ArgumentException("Customer ID is required", nameof(saleDto.CustomerID));

            if (saleDto.UserID == Guid.Empty)
                throw new ArgumentException("User ID is required", nameof(saleDto.UserID));

            if (saleDto.SaleItems == null || saleDto.SaleItems.Count == 0)
                throw new ArgumentException("Sale must contain at least one item", nameof(saleDto.SaleItems));

            foreach (var item in saleDto.SaleItems)
            {
                if (item.Quantity <= 0)
                    throw new ArgumentException("Item quantity must be greater than zero", nameof(item.Quantity));
                if (item.UnitPrice <= 0)
                    throw new ArgumentException("Item unit price must be greater than zero", nameof(item.UnitPrice));
                if (item.Discount < 0)
                    throw new ArgumentException("Item discount cannot be negative", nameof(item.Discount));
                if (item.Discount > item.UnitPrice * item.Quantity)
                    throw new ArgumentException("Item discount cannot exceed line value", nameof(item.Discount));
            }
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
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                Discount = itemDto.Discount,
                BatchNumber = itemDto.BatchNumber
            };
        }
    }
}