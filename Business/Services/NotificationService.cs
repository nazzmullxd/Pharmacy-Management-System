using Business.DTO;
using Business.Interfaces;
using Database.Interfaces;

namespace Business.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IProductBatchRepository _productBatchRepository;
        private readonly IProductRepository _productRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly Dictionary<string, bool> _notificationPreferences;

        public NotificationService(
            IProductBatchRepository productBatchRepository,
            IProductRepository productRepository,
            ISupplierRepository supplierRepository)
        {
            _productBatchRepository = productBatchRepository ?? throw new ArgumentNullException(nameof(productBatchRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));

            // Initialize default notification preferences
            _notificationPreferences = new Dictionary<string, bool>
            {
                { "ExpiryAlert", true },
                { "LowStockAlert", true },
                { "SaleNotification", false },
                { "PurchaseNotification", false }
            };
        }

        public async Task SendExpiryAlertAsync(ExpiryAlertDTO alert)
        {
            if (!await IsNotificationEnabledAsync("ExpiryAlert"))
                return;

            // Simulate sending an expiry alert
            Console.WriteLine($"EXPIRY ALERT: {alert.ProductName} (Batch: {alert.BatchNumber}) expires in {alert.DaysUntilExpiry} days. Stock: {alert.QuantityInStock}");
        }

        public async Task SendLowStockAlertAsync(ProductDTO product)
        {
            if (!await IsNotificationEnabledAsync("LowStockAlert"))
                return;

            // Simulate sending a low stock alert
            Console.WriteLine($"LOW STOCK ALERT: {product.ProductName} has only {product.TotalStock} units remaining (Threshold: {product.LowStockThreshold})");
        }

        public async Task SendSaleNotificationAsync(SaleDTO sale)
        {
            if (!await IsNotificationEnabledAsync("SaleNotification"))
                return;

            // Simulate sending a sale notification
            Console.WriteLine($"SALE NOTIFICATION: Sale #{sale.SaleID} completed for {sale.CustomerName}. Total: ${sale.TotalAmount:F2}");
        }

        public async Task SendPurchaseNotificationAsync(PurchaseDTO purchase)
        {
            if (!await IsNotificationEnabledAsync("PurchaseNotification"))
                return;

            // Simulate sending a purchase notification
            Console.WriteLine($"PURCHASE NOTIFICATION: Purchase #{purchase.PurchaseID} from {purchase.SupplierName}. Total: ${purchase.TotalAmount:F2}");
        }

        public async Task<IEnumerable<ExpiryAlertDTO>> GetPendingExpiryAlertsAsync()
        {
            var allBatches = await _productBatchRepository.GetAllAsync();
            var expiringBatches = allBatches.Where(b => b.ExpiryDate <= DateTime.UtcNow.AddDays(30) && b.QuantityInStock > 0);

            var alerts = new List<ExpiryAlertDTO>();
            foreach (var batch in expiringBatches)
            {
                var product = await _productRepository.GetByIdAsync(batch.ProductID);
                var supplier = await _supplierRepository.GetByIdAsync(batch.SupplierID);

                var daysUntilExpiry = (int)(batch.ExpiryDate - DateTime.UtcNow).TotalDays;
                var alertLevel = batch.ExpiryDate <= DateTime.UtcNow ? "Critical" :
                               batch.ExpiryDate <= DateTime.UtcNow.AddDays(7) ? "Warning" : "Info";

                alerts.Add(new ExpiryAlertDTO
                {
                    ProductBatchID = batch.ProductBatchID,
                    ProductID = batch.ProductID,
                    ProductName = product?.ProductName ?? string.Empty,
                    BatchNumber = batch.BatchNumber,
                    ExpiryDate = batch.ExpiryDate,
                    QuantityInStock = batch.QuantityInStock,
                    DaysUntilExpiry = daysUntilExpiry,
                    AlertLevel = alertLevel,
                    SupplierName = supplier?.SupplierName ?? string.Empty
                });
            }

            return alerts.OrderBy(a => a.ExpiryDate);
        }

        public async Task<IEnumerable<ProductDTO>> GetLowStockProductsAsync()
        {
            var allBatches = await _productBatchRepository.GetAllAsync();
            var lowStockBatches = allBatches.Where(b => b.QuantityInStock <= 10 && b.QuantityInStock > 0);

            var lowStockProducts = new Dictionary<Guid, int>();
            foreach (var batch in lowStockBatches)
            {
                if (lowStockProducts.ContainsKey(batch.ProductID))
                {
                    lowStockProducts[batch.ProductID] += batch.QuantityInStock;
                }
                else
                {
                    lowStockProducts[batch.ProductID] = batch.QuantityInStock;
                }
            }

            var productDtos = new List<ProductDTO>();
            foreach (var kvp in lowStockProducts)
            {
                var product = await _productRepository.GetByIdAsync(kvp.Key);
                if (product != null)
                {
                    productDtos.Add(new ProductDTO
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
                        TotalStock = kvp.Value,
                        LowStockThreshold = 10
                    });
                }
            }

            return productDtos;
        }

        public Task MarkAlertAsReadAsync(Guid alertId)
        {
            // Simulate marking an alert as read
            Console.WriteLine($"Alert {alertId} marked as read");
            return Task.CompletedTask;
        }

        public Task ClearAllAlertsAsync()
        {
            // Simulate clearing all alerts
            Console.WriteLine("All alerts cleared");
            return Task.CompletedTask;
        }

        public Task<bool> IsNotificationEnabledAsync(string notificationType)
        {
            // Check if the notification type is enabled
            return Task.FromResult(_notificationPreferences.TryGetValue(notificationType, out var enabled) && enabled);
        }

        public Task SetNotificationPreferenceAsync(string notificationType, bool enabled)
        {
            // Update the notification preference
            _notificationPreferences[notificationType] = enabled;
            Console.WriteLine($"Notification preference for {notificationType} set to {enabled}");
            return Task.CompletedTask;
        }

        public async Task SendBulkExpiryAlertsAsync()
        {
            var alerts = await GetPendingExpiryAlertsAsync();
            foreach (var alert in alerts.Where(a => a.AlertLevel == "Critical" || a.AlertLevel == "Warning"))
            {
                await SendExpiryAlertAsync(alert);
            }
        }

        public async Task SendBulkLowStockAlertsAsync()
        {
            var lowStockProducts = await GetLowStockProductsAsync();
            foreach (var product in lowStockProducts)
            {
                await SendLowStockAlertAsync(product);
            }
        }

        public async Task ProcessDailyAlertsAsync()
        {
            // Process all daily alerts
            await SendBulkExpiryAlertsAsync();
            await SendBulkLowStockAlertsAsync();
        }

        public Task<IEnumerable<string>> GetAvailableNotificationTypesAsync()
        {
            return Task.FromResult(_notificationPreferences.Keys.AsEnumerable());
        }

        public Task<Dictionary<string, bool>> GetAllNotificationPreferencesAsync()
        {
            return Task.FromResult(new Dictionary<string, bool>(_notificationPreferences));
        }
    }
}