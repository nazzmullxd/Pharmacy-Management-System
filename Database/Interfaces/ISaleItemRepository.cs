using Database.Model;
namespace Database.Interfaces
{
    public interface ISaleItemRepository
    {
        Task<IEnumerable<SaleItem>> GetAll();
        Task<SaleItem?> GetByIdAsync(Guid saleItemId);
        Task AddAsync(SaleItem saleItem);
        Task UpdateAsync(SaleItem saleItem);
        Task DeleteAsync(Guid saleItemId);
        Task<IEnumerable<SaleItem>> GetBySaleIdAsync(Guid saleId);
        Task<IEnumerable<SaleItem>> GetByProductIdAsync(Guid productId);


    }
}
