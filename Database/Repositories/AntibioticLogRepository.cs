using Database.Context;
using Database.Model;
using Database.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Database.Repositories
{
    public class AntibioticLogRepository
    {
        private readonly PharmacyManagementContext _context;
        public AntibioticLogRepository(PharmacyManagementContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<AntibioticLog>> GetAllAsync()
        {
            return await _context.AntibioticLogs.AsNoTracking().ToListAsync();
        }
        public async Task<AntibioticLog?> GetByIdAsync(Guid antibioticLogId)
        {
            return await _context.AntibioticLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(al => al.AntibioticLogID == antibioticLogId);
        }
        public async Task AddAsync(AntibioticLog antibioticLog)
        {
           await _context.AntibioticLogs.AddAsync(antibioticLog);
          await  _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(AntibioticLog antibioticLog)
        {
            if (antibioticLog == null)
            {
                throw new ArgumentNullException(nameof(antibioticLog), "Antibiotic log cannot be null.");
            }
            _context.AntibioticLogs.Update(antibioticLog);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Guid antibioticLogId)
        {
            var antibioticLog = await GetByIdAsync(antibioticLogId);
            if (antibioticLog == null)
            {
                throw new ArgumentNullException(nameof(antibioticLogId), $"Antibiotic log with ID ({antibioticLogId}) cannot be found.");
            }
            _context.AntibioticLogs.Remove(antibioticLog);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<AntibioticLog>> GetByProductBatchIdAsync(Guid productBatchId)
        {
            return await _context.AntibioticLogs
                .Where(al => al.ProductBatchID == productBatchId)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<IEnumerable<AntibioticLog>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.AntibioticLogs
                .Where(al => al.CustomerID == customerId)
                .AsNoTracking()
                .ToListAsync();
        }
       
    }
}
