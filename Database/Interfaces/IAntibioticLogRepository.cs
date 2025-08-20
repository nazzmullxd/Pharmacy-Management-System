using Database.Model;
namespace Database.Interfaces
{
    public interface IAntibioticLogRepository
    {
        Task<IEnumerable<AntibioticLog>> GetAllAsync();
        Task<AntibioticLog?> GetByIdAsync(Guid antibioticLogId);
        Task AddAsync(AntibioticLog antibioticLog);
        Task UpdateAsync(AntibioticLog antibioticLog);
        Task DeleteAsync(Guid antibioticLogId);
        Task<IEnumerable<AntibioticLog>> GetByProductBatchIdAsync(Guid productBatchId);
        Task<IEnumerable<AntibioticLog>> GetByCustomerIdAsync(Guid customerId);
    }
}
