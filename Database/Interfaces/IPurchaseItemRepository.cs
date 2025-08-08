using Database.Model;

namespace Database.Interfaces
{
    public interface IPurchaseItemRepository
    {
        Task<IEnumerable<PurchaseItem>> GetAllAsync();
        Task<PurchaseItem?> GetByIdAsync(Guid purchaseItemId);
        Task AddAsync(PurchaseItem purchaseItem);
        Task UpdateAsync(PurchaseItem purchaseItem);
        Task DeleteAsync(Guid purchaseItemId);
        Task<IEnumerable<PurchaseItem>> GetByPurchaseIdAsync(Guid purchaseId);
    }
}