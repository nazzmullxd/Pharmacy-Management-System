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
    public class DashboardService : IDashboardService
    {
        private readonly ISaleRepository _saleRepository;
        private readonly ISaleItemRepository _saleItemRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductBatchRepository _productBatchRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;

        public DashboardService(
            ISaleRepository saleRepository,
            ISaleItemRepository saleItemRepository,
            IPurchaseRepository purchaseRepository,
            IProductRepository productRepository,
            IProductBatchRepository productBatchRepository,
            ICustomerRepository customerRepository,
            IUserRepository userRepository)
        {
            _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
            _saleItemRepository = saleItemRepository ?? throw new ArgumentNullException(nameof(saleItemRepository));
            _purchaseRepository = purchaseRepository ?? throw new ArgumentNullException(nameof(purchaseRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _productBatchRepository = productBatchRepository ?? throw new ArgumentNullException(nameof(productBatchRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<DashboardKPIDTO> GetDashboardKPIsAsync(DateTime? date = null)
        {
            var targetDate = date ?? DateTime.UtcNow;
            var startOfDay = targetDate.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);
            var startOfMonth = new DateTime(targetDate.Year, targetDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var todaySales = await _saleRepository.GetByDateRangeAsync(startOfDay, endOfDay);
            var thisMonthSales = await _saleRepository.GetByDateRangeAsync(startOfMonth, endOfMonth);
            var allProducts = await _productRepository.GetAllAsync();
            var allBatches = await _productBatchRepository.GetAllAsync();

            return new DashboardKPIDTO
            {
                Date = targetDate,

                // Today's Metrics
                TodaySaleOrdersCount = todaySales.Count(),
                TodayInvoicesCount = todaySales.Count(s => s.PaymentStatus == "Paid"),
                TodaySalesAmount = todaySales.Sum(s => s.TotalAmount),
                TodayInvoiceAmount = todaySales.Where(s => s.PaymentStatus == "Paid").Sum(s => s.TotalAmount),

                // This Month's Metrics
                ThisMonthSaleOrdersCount = thisMonthSales.Count(),
                ThisMonthInvoicesCount = thisMonthSales.Count(s => s.PaymentStatus == "Paid"),
                ThisMonthSalesAmount = thisMonthSales.Sum(s => s.TotalAmount),
                ThisMonthInvoiceAmount = thisMonthSales.Where(s => s.PaymentStatus == "Paid").Sum(s => s.TotalAmount),

                // Stock Metrics
                StockItemsCount = allProducts.Count(p => p.IsActive),
                StockAdjustmentsCount = 0, // Would need to implement stock adjustments tracking
                TotalStockValue = await CalculateTotalStockValue(),
                LowStockItemsCount = await GetLowStockItemsCountAsync(),
                ExpiringItemsCount = await GetExpiringItemsCountAsync(),

                // Financial Metrics
                SalesDue = await CalculateSalesDue(),
                InvoiceDue = await CalculateInvoiceDue(),
                TotalDue = await CalculateSalesDue() + await CalculateInvoiceDue(),
                NetProfit = await CalculateNetProfit(),

                // Support Metrics
                SupportTicketsCount = 0, // Would need to implement support tickets
                OpenSupportTicketsCount = 0,

                // Performance Indicators
                SalesGrowthPercentage = await CalculateSalesGrowthPercentage(),
                ProfitMarginPercentage = await CalculateProfitMarginPercentage(),
                TopSellingProductsCount = 10
            };
        }

        public Task<DashboardKPIDTO> GetTodayKPIsAsync()
        {
            return GetDashboardKPIsAsync(DateTime.UtcNow);
        }

        public async Task<DashboardKPIDTO> GetThisMonthKPIsAsync()
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var thisMonthSales = await _saleRepository.GetByDateRangeAsync(startOfMonth, endOfMonth);

            return new DashboardKPIDTO
            {
                Date = now,
                ThisMonthSaleOrdersCount = thisMonthSales.Count(),
                ThisMonthInvoicesCount = thisMonthSales.Count(s => s.PaymentStatus == "Paid"),
                ThisMonthSalesAmount = thisMonthSales.Sum(s => s.TotalAmount),
                ThisMonthInvoiceAmount = thisMonthSales.Where(s => s.PaymentStatus == "Paid").Sum(s => s.TotalAmount),
                NetProfit = await CalculateNetProfit(),
                TotalStockValue = await CalculateTotalStockValue()
            };
        }

        public async Task<IEnumerable<TopProductDTO>> GetTopSellingProductsAsync(int count = 10)
        {
            var lastMonth = DateTime.UtcNow.AddMonths(-1);
            var sales = await _saleRepository.GetByDateRangeAsync(lastMonth, DateTime.UtcNow);

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

        public async Task<IEnumerable<ProductDTO>> GetTopStockProductsAsync(int count = 10)
        {
            var allBatches = await _productBatchRepository.GetAllAsync();
            var productStock = new Dictionary<Guid, int>();

            foreach (var batch in allBatches.Where(b => b.ExpiryDate > DateTime.UtcNow))
            {
                if (productStock.ContainsKey(batch.ProductID))
                {
                    productStock[batch.ProductID] += batch.QuantityInStock;
                }
                else
                {
                    productStock[batch.ProductID] = batch.QuantityInStock;
                }
            }

            var topStockProducts = productStock
                .OrderByDescending(ps => ps.Value)
                .Take(count);

            var result = new List<ProductDTO>();
            foreach (var kvp in topStockProducts)
            {
                var product = await _productRepository.GetByIdAsync(kvp.Key);
                if (product != null)
                {
                    result.Add(new ProductDTO
                    {
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,
                        GenericName = product.GenericName,
                        Manufacturer = product.Manufacturer,
                        Category = product.Category,
                        Description = product.Description,
                        UnitPrice = product.UnitPrice,
                        DefaultRetailPrice = product.DefaultRetailPrice,
                        DefaultWholeSalePrice = product.DefaultWholeSalePrice,
                        IsActive = product.IsActive,
                        Barcode = product.Barcode,
                        CreatedDate = product.CreatedDate,
                        TotalStock = kvp.Value
                    });
                }
            }

            return result;
        }

        public async Task<IEnumerable<ExpiryAlertDTO>> GetExpiringProductsAsync(int daysAhead = 30)
        {
            var allBatches = await _productBatchRepository.GetAllAsync();
            var expiringBatches = allBatches.Where(b => b.ExpiryDate <= DateTime.UtcNow.AddDays(daysAhead) && b.QuantityInStock > 0);

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

        public async Task<IEnumerable<SaleDTO>> GetRecentSalesAsync(int count = 5)
        {
            var sales = await _saleRepository.GetAllAsync();
            var recentSales = sales.OrderByDescending(s => s.SaleDate).Take(count);

            var saleDtos = new List<SaleDTO>();
            foreach (var sale in recentSales)
            {
                Customer? customer = null;
                if (sale.CustomerID.HasValue && sale.CustomerID.Value != Guid.Empty)
                {
                    customer = await _customerRepository.GetByIdAsync(sale.CustomerID.Value);
                }
                var user = await _userRepository.GetByIdAsync(sale.UserID);

                saleDtos.Add(new SaleDTO
                {
                    SaleID = sale.SaleID,
                    CustomerID = sale.CustomerID ?? Guid.Empty,
                    CustomerName = customer?.CustomerName ?? "Walk-in Customer",
                    UserID = sale.UserID,
                    UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown User",
                    SaleDate = sale.SaleDate,
                    TotalAmount = sale.TotalAmount,
                    PaymentStatus = sale.PaymentStatus ?? "Unknown",
                    Note = sale.Note ?? string.Empty
                });
            }

            return saleDtos;
        }

        public Task<IEnumerable<StockAdjustmentDTO>> GetRecentAdjustmentsAsync(int count = 5)
        {
            // In a real implementation, this would query recent stock adjustments
            return Task.FromResult<IEnumerable<StockAdjustmentDTO>>(new List<StockAdjustmentDTO>());
        }

        public Task<decimal> GetTotalStockValueAsync()
        {
            return CalculateTotalStockValue();
        }

        public Task<decimal> GetTotalSalesDueAsync()
        {
            return CalculateSalesDue();
        }

        public Task<decimal> GetTotalInvoiceDueAsync()
        {
            return CalculateInvoiceDue();
        }

        public Task<int> GetLowStockItemsCountAsync(int threshold = 10)
        {
            return GetLowStockItemsCountAsync();
        }

        public Task<int> GetExpiringItemsCountAsync(int daysAhead = 30)
        {
            return GetExpiringItemsCountAsync();
        }

        // Helper methods
        private async Task<decimal> CalculateTotalStockValue()
        {
            var allBatches = await _productBatchRepository.GetAllAsync();
            var totalValue = 0m;

            foreach (var batch in allBatches.Where(b => b.ExpiryDate > DateTime.UtcNow))
            {
                var product = await _productRepository.GetByIdAsync(batch.ProductID);
                if (product != null)
                {
                    totalValue += product.UnitPrice * batch.QuantityInStock;
                }
            }

            return totalValue;
        }

        private async Task<decimal> CalculateSalesDue()
        {
            var sales = await _saleRepository.GetAllAsync();
            return sales.Where(s => s.PaymentStatus == "Pending").Sum(s => s.TotalAmount);
        }

        private async Task<decimal> CalculateInvoiceDue()
        {
            var purchases = await _purchaseRepository.GetAllAsync();
            return purchases.Where(p => p.PaymentStatus == "Pending").Sum(p => p.TotalAmount);
        }

        private async Task<decimal> CalculateNetProfit()
        {
            var sales = await _saleRepository.GetAllAsync();
            var purchases = await _purchaseRepository.GetAllAsync();

            var totalSales = sales.Sum(s => s.TotalAmount);
            var totalPurchases = purchases.Sum(p => p.TotalAmount);

            return totalSales - totalPurchases;
        }

        private async Task<decimal> CalculateSalesGrowthPercentage()
        {
            var now = DateTime.UtcNow;
            var thisMonth = new DateTime(now.Year, now.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);
            var lastMonthEnd = thisMonth.AddDays(-1);

            var thisMonthSales = await _saleRepository.GetByDateRangeAsync(thisMonth, now);
            var lastMonthSales = await _saleRepository.GetByDateRangeAsync(lastMonth, lastMonthEnd);

            var thisMonthTotal = thisMonthSales.Sum(s => s.TotalAmount);
            var lastMonthTotal = lastMonthSales.Sum(s => s.TotalAmount);

            if (lastMonthTotal == 0) return 0;

            return ((thisMonthTotal - lastMonthTotal) / lastMonthTotal) * 100;
        }

        private async Task<decimal> CalculateProfitMarginPercentage()
        {
            var netProfit = await CalculateNetProfit();
            var totalSales = (await _saleRepository.GetAllAsync()).Sum(s => s.TotalAmount);

            if (totalSales == 0) return 0;

            return (netProfit / totalSales) * 100;
        }

        private async Task<int> GetLowStockItemsCountAsync()
        {
            var allBatches = await _productBatchRepository.GetAllAsync();
            var productStock = new Dictionary<Guid, int>();

            foreach (var batch in allBatches.Where(b => b.ExpiryDate > DateTime.UtcNow))
            {
                if (productStock.ContainsKey(batch.ProductID))
                {
                    productStock[batch.ProductID] += batch.QuantityInStock;
                }
                else
                {
                    productStock[batch.ProductID] = batch.QuantityInStock;
                }
            }

            return productStock.Count(ps => ps.Value <= 10);
        }

        private async Task<int> GetExpiringItemsCountAsync()
        {
            var allBatches = await _productBatchRepository.GetAllAsync();
            return allBatches.Count(b => b.ExpiryDate <= DateTime.UtcNow.AddDays(30) && b.QuantityInStock > 0);
        }
    }
}