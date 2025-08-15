using Database.Model;
namespace Database.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<IEnumerable<AuditLog>> GetAllAsync();
        Task<AuditLog?> GetByIdAsync(Guid auditLogId);
        Task AddAsync(AuditLog auditLog);
        Task UpdateAsync(AuditLog auditLog);
        Task DeleteAsync(Guid auditLogId);
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<AuditLog>> GetByActionTypeAsync(string actionType);
    }
}
