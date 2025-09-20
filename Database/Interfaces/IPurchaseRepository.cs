using Database.Model;

namespace Database.Interfaces
{
    public interface IPurchaseRepository
    {
        Task<IEnumerable<Purchase>> GetAllAsync();
        Task<Purchase?> GetByIdAsync(Guid purchaseId);
        Task AddAsync(Purchase purchase);
        Task UpdateAsync(Purchase purchase);
        Task DeleteAsync(Guid purchaseId);
        Task<IEnumerable<Purchase>> GetBySupplierIdAsync(Guid supplierId);
      //  Task<IEnumerable<Purchase>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Purchase>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}