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
            var saleDtos = new List<SaleDTO>();

            foreach (var sale in sales)
            {
                var saleDto = await MapToDTO(sale);
                saleDtos.Add(saleDto);
            }

            return saleDtos;
        }

        public async Task<SaleDTO?> GetSaleByIdAsync(Guid saleId)
        {
            var sale = await _saleRepository.GetByIdAsync(saleId);
            return sale != null ? await MapToDTO(sale) : null;
        }

        public async Task<SaleDTO> CreateSaleAsync(SaleDTO saleDto)
        {
            if (saleDto == null)
                throw new ArgumentNullException(nameof(saleDto));

            ValidateSale(saleDto);

            var sale = MapToEntity(saleDto);
            sale.SaleID = Guid.NewGuid();
            sale.SaleDate = DateTime.UtcNow;

            await _saleRepository.AddAsync(sale);

            // Add sale items
            foreach (var itemDto in saleDto.SaleItems)
            {
                var saleItem = MapToSaleItemEntity(itemDto, sale.SaleID);
                await _saleItemRepository.AddAsync(saleItem);
            }

            return await MapToDTO(sale);
        }

        public async Task<SaleDTO> UpdateSaleAsync(SaleDTO saleDto)
        {
            if (saleDto == null)
                throw new ArgumentNullException(nameof(saleDto));

            var existingSale = await _saleRepository.GetByIdAsync(saleDto.SaleID);
            if (existingSale == null)
                throw new ArgumentException("Sale not found", nameof(saleDto.SaleID));

            ValidateSale(saleDto);

            var sale = MapToEntity(saleDto);
            sale.SaleDate = existingSale.SaleDate; // Preserve original sale date

            await _saleRepository.UpdateAsync(sale);

            // Update sale items (remove existing and add new ones)
            var existingItems = await _saleItemRepository.GetBySaleIdAsync(sale.SaleID);
            foreach (var item in existingItems)
            {
                await _saleItemRepository.DeleteAsync(item.SaleItemID);
            }

            foreach (var itemDto in saleDto.SaleItems)
            {
                var saleItem = MapToSaleItemEntity(itemDto, sale.SaleID);
                await _saleItemRepository.AddAsync(saleItem);
            }

            return await MapToDTO(sale);
        }

        public async Task<bool> DeleteSaleAsync(Guid saleId)
        {
            try
            {
                // Delete sale items first
                var saleItems = await _saleItemRepository.GetBySaleIdAsync(saleId);
                foreach (var item in saleItems)
                {
                    await _saleItemRepository.DeleteAsync(item.SaleItemID);
                }

                // Delete sale
                await _saleRepository.DeleteAsync(saleId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<SaleDTO>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var sales = await _saleRepository.GetByDateRangeAsync(startDate, endDate);
            var saleDtos = new List<SaleDTO>();

            foreach (var sale in sales)
            {
                var saleDto = await MapToDTO(sale);
                saleDtos.Add(saleDto);
            }

            return saleDtos;
        }

        public async Task<IEnumerable<SaleDTO>> GetSalesByCustomerAsync(Guid customerId)
        {
            var sales = await _saleRepository.GetByCustomerIdAsync(customerId);
            var saleDtos = new List<SaleDTO>();

            foreach (var sale in sales)
            {
                var saleDto = await MapToDTO(sale);
                saleDtos.Add(saleDto);
            }

            return saleDtos;
        }

        public async Task<IEnumerable<SaleDTO>> GetSalesByUserAsync(Guid userId)
        {
            var sales = await _saleRepository.GetByUserIdAsync(userId);
            var saleDtos = new List<SaleDTO>();

            foreach (var sale in sales)
            {
                var saleDto = await MapToDTO(sale);
                saleDtos.Add(saleDto);
            }

            return saleDtos;
        }

        public async Task<decimal> GetTotalSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var sales = await _saleRepository.GetByDateRangeAsync(startDate, endDate);
            return sales.Sum(s => s.TotalAmount);
        }

        public async Task<IEnumerable<TopProductDTO>> GetTopSellingProductsAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null)
        {
            var sales = startDate.HasValue && endDate.HasValue
                ? await _saleRepository.GetByDateRangeAsync(startDate.Value, endDate.Value)
                : await _saleRepository.GetAllAsync();

            var productSales = new Dictionary<Guid, (string ProductName, string Category, int TotalQuantity, decimal TotalRevenue)>();

            foreach (var sale in sales)
            {
                var saleItems = await _saleItemRepository.GetBySaleIdAsync(sale.SaleID);
                foreach (var item in saleItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductID);
                    if (product != null)
                    {
                        if (productSales.ContainsKey(item.ProductID))
                        {
                            var existing = productSales[item.ProductID];
                            productSales[item.ProductID] = (
                                existing.ProductName,
                                existing.Category,
                                existing.TotalQuantity + item.Quantity,
                                existing.TotalRevenue + item.TotalPrice
                            );
                        }
                        else
                        {
                            productSales[item.ProductID] = (
                                product.ProductName,
                                product.Category,
                                item.Quantity,
                                item.TotalPrice
                            );
                        }
                    }
                }
            }

            return productSales
                .OrderByDescending(ps => ps.Value.TotalQuantity)
                .Take(count)
                .Select((ps, index) => new TopProductDTO
                {
                    ProductID = ps.Key,
                    ProductName = ps.Value.ProductName,
                    Category = ps.Value.Category,
                    TotalQuantitySold = ps.Value.TotalQuantity,
                    TotalRevenue = ps.Value.TotalRevenue,
                    Rank = index + 1
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
            try
            {
                var sale = await _saleRepository.GetByIdAsync(saleId);
                if (sale == null)
                    return false;

                sale.PaymentSatus = paymentStatus;
                await _saleRepository.UpdateAsync(sale);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ValidateSale(SaleDTO saleDto)
        {
            if (saleDto.CustomerID == Guid.Empty)
                throw new ArgumentException("Customer ID is required", nameof(saleDto.CustomerID));

            if (saleDto.UserID == Guid.Empty)
                throw new ArgumentException("User ID is required", nameof(saleDto.UserID));

            if (saleDto.TotalAmount <= 0)
                throw new ArgumentException("Total amount must be greater than zero", nameof(saleDto.TotalAmount));

            if (saleDto.SaleItems == null || !saleDto.SaleItems.Any())
                throw new ArgumentException("Sale must have at least one item", nameof(saleDto.SaleItems));

            foreach (var item in saleDto.SaleItems)
            {
                if (item.Quantity <= 0)
                    throw new ArgumentException("Item quantity must be greater than zero", nameof(item.Quantity));

                if (item.UnitPrice <= 0)
                    throw new ArgumentException("Item unit price must be greater than zero", nameof(item.UnitPrice));
            }
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
                saleItemDtos.Add(new SaleItemDTO
                {
                    SaleItemID = item.SaleItemID,
                    SaleID = item.SaleID,
                    ProductID = item.ProductID,
                    ProductName = product?.ProductName ?? string.Empty,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    BatchNumber = item.BatchNumber
                });
            }

            return new SaleDTO
            {
                SaleID = sale.SaleID,
                CustomerID = sale.CustomerID,
                CustomerName = customer?.CustomerName ?? string.Empty,
                UserID = sale.UserID,
                UserName = user != null ? $"{user.FirstName} {user.LastName}" : string.Empty,
                SaleDate = sale.SaleDate,
                TotalAmount = sale.TotalAmount,
                PaymentStatus = sale.PaymentSatus,
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
                PaymentSatus = saleDto.PaymentStatus,
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
                TotalPrice = itemDto.TotalPrice,
                BatchNumber = itemDto.BatchNumber
            };
        }
    }
}
