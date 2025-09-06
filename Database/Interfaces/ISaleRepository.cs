using Database.Model;
namespace Database.Interfaces
{
    public interface ISaleRepository
    {
        Task<IEnumerable<Sale>> GetAllAsync();
        Task<Sale?> GetByIdAsync(Guid saleId);
        Task AddAsync(Sale sale);
        Task UpdateAsync(Sale sale);
        Task DeleteAsync(Guid saleId);
        Task<IEnumerable<Sale>> GetByCustomerIdAsync(Guid customerId);
        Task<IEnumerable<Sale>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
