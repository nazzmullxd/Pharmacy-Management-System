using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.DTO;
using Business.Interfaces;
using Database.Interfaces;

namespace Business.Services
{
    public class ReportService : IReportService
    {
        private readonly ISaleRepository _saleRepository;
        private readonly ISaleItemRepository _saleItemRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IPurchaseItemRepository _purchaseItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductBatchRepository _productBatchRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public ReportService(
            ISaleRepository saleRepository,
            ISaleItemRepository saleItemRepository,
            IPurchaseRepository purchaseRepository,
            IPurchaseItemRepository purchaseItemRepository,
            IProductRepository productRepository,
            IProductBatchRepository productBatchRepository,
            ICustomerRepository customerRepository,
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository)
        {
            _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
            _saleItemRepository = saleItemRepository ?? throw new ArgumentNullException(nameof(saleItemRepository));
            _purchaseRepository = purchaseRepository ?? throw new ArgumentNullException(nameof(purchaseRepository));
            _purchaseItemRepository = purchaseItemRepository ?? throw new ArgumentNullException(nameof(purchaseItemRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _productBatchRepository = productBatchRepository ?? throw new ArgumentNullException(nameof(productBatchRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
        }

        public async Task<ReportDTO> GenerateSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            var sales = await _saleRepository.GetByDateRangeAsync(startDate, endDate);
            var purchases = await _purchaseRepository.GetByDateRangeAsync(startDate, endDate);
            
            var totalSales = sales.Sum(s => s.TotalAmount);
            var totalPurchases = purchases.Sum(p => p.TotalAmount);
            var netProfit = totalSales - totalPurchases;
            var totalTransactions = sales.Count();

            var topProducts = await GetTopSellingProductsReportAsync(10, startDate, endDate);
            var recentSales = await GetRecentSalesAsync(10);
            var expiringProducts = await GetExpiryAlertsAsync();

            return new ReportDTO
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalSales = totalSales,
                TotalPurchases = totalPurchases,
                NetProfit = netProfit,
                TotalTransactions = totalTransactions,
                TopSellingProducts = topProducts.ToList(),
                RecentSales = recentSales.ToList(),
                ExpiringProducts = expiringProducts.ToList()
            };
        }

        public async Task<ReportDTO> GenerateInventoryReportAsync()
        {
            var allBatches = await _productBatchRepository.GetAllAsync();
            var expiringBatches = allBatches.Where(b => b.ExpiryDate <= DateTime.UtcNow.AddDays(30) && b.QuantityInStock > 0);
            var lowStockBatches = allBatches.Where(b => b.QuantityInStock <= 10 && b.QuantityInStock > 0);

            var expiringProducts = new List<ExpiryAlertDTO>();
            foreach (var batch in expiringBatches)
            {
                var product = await _productRepository.GetByIdAsync(batch.ProductID);
                var supplier = await _productBatchRepository.GetByIdAsync(batch.ProductBatchID);
                
                expiringProducts.Add(new ExpiryAlertDTO
                {
                    ProductBatchID = batch.ProductBatchID,
                    ProductID = batch.ProductID,
                    ProductName = product?.ProductName ?? string.Empty,
                    BatchNumber = batch.BatchNumber,
                    ExpiryDate = batch.ExpiryDate,
                    QuantityInStock = batch.QuantityInStock,
                    DaysUntilExpiry = (int)(batch.ExpiryDate - DateTime.UtcNow).TotalDays,
                    AlertLevel = batch.ExpiryDate <= DateTime.UtcNow ? "Critical" : "Warning",
                    SupplierName = string.Empty // Would need supplier lookup
                });
            }

            return new ReportDTO
            {
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                TotalSales = 0,
                TotalPurchases = 0,
                NetProfit = 0,
                TotalTransactions = 0,
                TopSellingProducts = new List<TopProductDTO>(),
                RecentSales = new List<SaleDTO>(),
                ExpiringProducts = expiringProducts
            };
        }

        public async Task<IEnumerable<TopProductDTO>> GetTopSellingProductsReportAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null)
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

        public async Task<IEnumerable<ExpiryAlertDTO>> GetExpiryReportAsync()
        {
            var allBatches = await _productBatchRepository.GetAllAsync();
            var expiringBatches = allBatches.Where(b => b.ExpiryDate <= DateTime.UtcNow.AddDays(30) && b.QuantityInStock > 0);
            
            var alerts = new List<ExpiryAlertDTO>();
            foreach (var batch in expiringBatches)
            {
                var product = await _productRepository.GetByIdAsync(batch.ProductID);
                
                alerts.Add(new ExpiryAlertDTO
                {
                    ProductBatchID = batch.ProductBatchID,
                    ProductID = batch.ProductID,
                    ProductName = product?.ProductName ?? string.Empty,
                    BatchNumber = batch.BatchNumber,
                    ExpiryDate = batch.ExpiryDate,
                    QuantityInStock = batch.QuantityInStock,
                    DaysUntilExpiry = (int)(batch.ExpiryDate - DateTime.UtcNow).TotalDays,
                    AlertLevel = batch.ExpiryDate <= DateTime.UtcNow ? "Critical" : 
                                batch.ExpiryDate <= DateTime.UtcNow.AddDays(7) ? "Warning" : "Info",
                    SupplierName = string.Empty // Would need supplier lookup
                });
            }

            return alerts.OrderBy(a => a.ExpiryDate);
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            var sales = await _saleRepository.GetByDateRangeAsync(startDate, endDate);
            return sales.Sum(s => s.TotalAmount);
        }

        public async Task<decimal> GetTotalProfitAsync(DateTime startDate, DateTime endDate)
        {
            var totalSales = await GetTotalRevenueAsync(startDate, endDate);
            var purchases = await _purchaseRepository.GetByDateRangeAsync(startDate, endDate);
            var totalPurchases = purchases.Sum(p => p.TotalAmount);
            
            return totalSales - totalPurchases;
        }

        public async Task<IEnumerable<SaleDTO>> GetDailySalesReportAsync(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);
            
            var sales = await _saleRepository.GetByDateRangeAsync(startOfDay, endOfDay);
            var saleDtos = new List<SaleDTO>();

            foreach (var sale in sales)
            {
                var saleDto = await MapSaleToDTO(sale);
                saleDtos.Add(saleDto);
            }

            return saleDtos;
        }

        public async Task<IEnumerable<SaleDTO>> GetMonthlySalesReportAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            var sales = await _saleRepository.GetByDateRangeAsync(startDate, endDate);
            var saleDtos = new List<SaleDTO>();

            foreach (var sale in sales)
            {
                var saleDto = await MapSaleToDTO(sale);
                saleDtos.Add(saleDto);
            }

            return saleDtos;
        }

        public async Task<ReportDTO> GenerateCustomerReportAsync(Guid customerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var sales = startDate.HasValue && endDate.HasValue
                ? await _saleRepository.GetByDateRangeAsync(startDate.Value, endDate.Value)
                : await _saleRepository.GetAllAsync();

            var customerSales = sales.Where(s => s.CustomerID == customerId);
            var totalSales = customerSales.Sum(s => s.TotalAmount);
            var totalTransactions = customerSales.Count();

            var customerSaleDtos = new List<SaleDTO>();
            foreach (var sale in customerSales)
            {
                var saleDto = await MapSaleToDTO(sale);
                customerSaleDtos.Add(saleDto);
            }

            return new ReportDTO
            {
                StartDate = startDate ?? DateTime.UtcNow.AddDays(-30),
                EndDate = endDate ?? DateTime.UtcNow,
                TotalSales = totalSales,
                TotalPurchases = 0,
                NetProfit = totalSales,
                TotalTransactions = totalTransactions,
                TopSellingProducts = new List<TopProductDTO>(),
                RecentSales = customerSaleDtos,
                ExpiringProducts = new List<ExpiryAlertDTO>()
            };
        }
/*
        public async Task<IEnumerable<AuditLogDTO>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var auditLogs = startDate.HasValue && endDate.HasValue
                ? await _auditLogRepository.GetByDateRangeAsync(startDate.Value, endDate.Value)
                : await _auditLogRepository.GetAllAsync();

            var auditLogDtos = new List<AuditLogDTO>();
            foreach (var log in auditLogs)
            {
                var user = await _userRepository.GetByIdAsync(log.UserID);
                auditLogDtos.Add(new AuditLogDTO
                {
                    AuditLogID = log.AuditLogID,
                    UserID = log.UserID,
                    UserName = user != null ? $"{user.FirstName} {user.LastName}" : string.Empty,
                    Action = log.Action,
                });
            }

            return auditLogDtos.OrderByDescending(a => a.Timestamp);
        }
*/
        private async Task<IEnumerable<SaleDTO>> GetRecentSalesAsync(int count)
        {
            var sales = await _saleRepository.GetAllAsync();
            var recentSales = sales.OrderByDescending(s => s.SaleDate).Take(count);
            
            var saleDtos = new List<SaleDTO>();
            foreach (var sale in recentSales)
            {
                var saleDto = await MapSaleToDTO(sale);
                saleDtos.Add(saleDto);
            }

            return saleDtos;
        }

        private async Task<IEnumerable<ExpiryAlertDTO>> GetExpiryAlertsAsync()
        {
            var allBatches = await _productBatchRepository.GetAllAsync();
            var expiringBatches = allBatches.Where(b => b.ExpiryDate <= DateTime.UtcNow.AddDays(30) && b.QuantityInStock > 0);
            
            var alerts = new List<ExpiryAlertDTO>();
            foreach (var batch in expiringBatches)
            {
                var product = await _productRepository.GetByIdAsync(batch.ProductID);
                
                alerts.Add(new ExpiryAlertDTO
                {
                    ProductBatchID = batch.ProductBatchID,
                    ProductID = batch.ProductID,
                    ProductName = product?.ProductName ?? string.Empty,
                    BatchNumber = batch.BatchNumber,
                    ExpiryDate = batch.ExpiryDate,
                    QuantityInStock = batch.QuantityInStock,
                    DaysUntilExpiry = (int)(batch.ExpiryDate - DateTime.UtcNow).TotalDays,
                    AlertLevel = batch.ExpiryDate <= DateTime.UtcNow ? "Critical" : 
                                batch.ExpiryDate <= DateTime.UtcNow.AddDays(7) ? "Warning" : "Info",
                    SupplierName = string.Empty // Would need supplier lookup
                });
            }

            return alerts.OrderBy(a => a.ExpiryDate);
        }

        private async Task<SaleDTO> MapSaleToDTO(Database.Model.Sale sale)
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
                PaymentStatus = sale.PaymentStatus,
                Note = sale.Note,
                SaleItems = saleItemDtos
            };
        }
    }
}
