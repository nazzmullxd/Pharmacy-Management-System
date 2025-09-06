using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            // In a real implementation, this would send actual notifications
            // For now, we'll just log the alert
            Console.WriteLine($"EXPIRY ALERT: {alert.ProductName} (Batch: {alert.BatchNumber}) expires in {alert.DaysUntilExpiry} days. Stock: {alert.QuantityInStock}");
            
            // Here you could integrate with:
            // - Email service
            // - SMS service
            // - Push notifications
            // - In-app notifications
            // - Webhook calls
        }

        public async Task SendLowStockAlertAsync(ProductDTO product)
        {
            if (!await IsNotificationEnabledAsync("LowStockAlert"))
                return;

            // In a real implementation, this would send actual notifications
            Console.WriteLine($"LOW STOCK ALERT: {product.ProductName} has only {product.TotalStock} units remaining (Threshold: {product.LowStockThreshold})");
            
            // Here you could integrate with notification services
        }

        public async Task SendSaleNotificationAsync(SaleDTO sale)
        {
            if (!await IsNotificationEnabledAsync("SaleNotification"))
                return;

            // In a real implementation, this would send actual notifications
            Console.WriteLine($"SALE NOTIFICATION: Sale #{sale.SaleID} completed for {sale.CustomerName}. Total: ${sale.TotalAmount:F2}");
            
            // Here you could integrate with notification services
        }

        public async Task SendPurchaseNotificationAsync(PurchaseDTO purchase)
        {
            if (!await IsNotificationEnabledAsync("PurchaseNotification"))
                return;

            // In a real implementation, this would send actual notifications
            Console.WriteLine($"PURCHASE NOTIFICATION: Purchase #{purchase.PurchaseID} from {purchase.SupplierName}. Total: ${purchase.TotalAmount:F2}");
            
            // Here you could integrate with notification services
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

        public async Task MarkAlertAsReadAsync(Guid alertId)
        {
            // In a real implementation, this would mark the alert as read in the database
            // For now, we'll just log the action
            Console.WriteLine($"Alert {alertId} marked as read");
        }

        public async Task ClearAllAlertsAsync()
        {
            // In a real implementation, this would clear all alerts from the database
            // For now, we'll just log the action
            Console.WriteLine("All alerts cleared");
        }

        public async Task<bool> IsNotificationEnabledAsync(string notificationType)
        {
            // In a real implementation, this would check user preferences from the database
            // For now, we'll use the in-memory dictionary
            return _notificationPreferences.TryGetValue(notificationType, out var enabled) && enabled;
        }

        public async Task SetNotificationPreferenceAsync(string notificationType, bool enabled)
        {
            // In a real implementation, this would save the preference to the database
            // For now, we'll update the in-memory dictionary
            _notificationPreferences[notificationType] = enabled;
            Console.WriteLine($"Notification preference for {notificationType} set to {enabled}");
        }

        // Additional helper methods for comprehensive notification management
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
            // This method could be called by a scheduled job to process all daily alerts
            await SendBulkExpiryAlertsAsync();
            await SendBulkLowStockAlertsAsync();
        }

        public async Task<IEnumerable<string>> GetAvailableNotificationTypesAsync()
        {
            return await Task.FromResult(_notificationPreferences.Keys);
        }

        public async Task<Dictionary<string, bool>> GetAllNotificationPreferencesAsync()
        {
            return await Task.FromResult(new Dictionary<string, bool>(_notificationPreferences));
        }
    }
}
