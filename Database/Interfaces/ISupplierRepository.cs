using Database.Model;
namespace Database.Interfaces
{
    public interface ISupplierRepository
    {
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task<Supplier?> GetByIdAsync(Guid supplierId);
        Task AddAsync(Supplier supplier);
        Task UpdateAsync(Supplier supplier);
        Task DeleteAsync(Guid supplierId);
        Task<IEnumerable<Supplier>> GetByNameAsync(string name);
        Task<IEnumerable<Supplier>> GetByContactInfoAsync(string contactInfo);
        Task<IEnumerable<Supplier>> GetByLocationAsync(string location);
    }
}
