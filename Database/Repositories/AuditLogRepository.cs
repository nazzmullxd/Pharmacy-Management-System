using Database.Context;
using Database.Model;
using Database.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace Database.Repositories
{
    public class AuditLogRepository: IAuditLogRepository
    {
        private readonly PharmacyManagementContext _context;
        public AuditLogRepository(PharmacyManagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync()
        {
            return await _context.AuditLogs.AsNoTracking().ToListAsync();
        }
        public async Task<AuditLog?> GetByIdAsync(Guid auditLogId)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(al => al.AuditLogID == auditLogId);
        }
        public async Task AddAsync(AuditLog auditLog)
        {
            if (auditLog == null)
            {
                throw new ArgumentNullException(nameof(auditLog), "Audit log cannot be null.");
            }
            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(AuditLog auditLog)
        {
            if (auditLog == null)
            {
                throw new ArgumentNullException(nameof(auditLog), "Audit log cannot be null.");
            }
            _context.AuditLogs.Update(auditLog);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Guid auditLogId)
        {
            var auditLog = await GetByIdAsync(auditLogId);
            if (auditLog == null)
            {
                throw new ArgumentNullException(nameof(auditLogId), $"Audit log with ID ({auditLogId}) cannot be found.");
            }
            _context.AuditLogs.Remove(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AuditLogs
                .Where(al => al.ActionDate >= startDate && al.ActionDate <= endDate)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}