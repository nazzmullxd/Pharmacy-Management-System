using Database.Model;

namespace Database.Interfaces
{
    public interface IProductBatchRepository
    {
        Task<IEnumerable<ProductBatch>> GetAllAsync();
        Task<ProductBatch?> GetByIdAsync(Guid productBatchId);
        Task AddAsync(ProductBatch productBatch);
        Task UpdateAsync(ProductBatch productBatch);
        Task DeleteAsync(Guid productBatchId);
        Task<IEnumerable<ProductBatch>> GetByProductIdAsync(Guid productId);
    }
}