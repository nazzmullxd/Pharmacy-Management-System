using Database.Model;
namespace Database.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserInfo>> GetAllAsync();
        Task<UserInfo?> GetByIdAsync(Guid userId);
        Task AddAsync(UserInfo user);
        Task UpdateAsync(UserInfo user);
        Task DeleteAsync(Guid userId);
        Task<IEnumerable<UserInfo>> GetByUsernameAsync(string username);
        Task<UserInfo?> GetByEmailAsync(string email);
        Task<IEnumerable<UserInfo>> GetByRoleAsync(string role);
    }
}
