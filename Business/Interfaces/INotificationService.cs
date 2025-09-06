using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Business.DTO;

namespace Business.Interfaces
{
    public interface INotificationService
    {
        Task SendExpiryAlertAsync(ExpiryAlertDTO alert);
        Task SendLowStockAlertAsync(ProductDTO product);
        Task SendSaleNotificationAsync(SaleDTO sale);
        Task SendPurchaseNotificationAsync(PurchaseDTO purchase);
        Task<IEnumerable<ExpiryAlertDTO>> GetPendingExpiryAlertsAsync();
        Task<IEnumerable<ProductDTO>> GetLowStockProductsAsync();
        Task MarkAlertAsReadAsync(Guid alertId);
        Task ClearAllAlertsAsync();
        Task<bool> IsNotificationEnabledAsync(string notificationType);
        Task SetNotificationPreferenceAsync(string notificationType, bool enabled);
    }
}
