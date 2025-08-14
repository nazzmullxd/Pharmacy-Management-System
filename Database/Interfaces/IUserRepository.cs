using Database.Model;
namespace Database.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(Guid userId);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(Guid userId);
        Task<IEnumerable<User>> GetByUsernameAsync(string username);
        Task<IEnumerable<User>> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetByRoleAsync(string role);
    }
}
