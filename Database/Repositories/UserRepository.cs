using Database.Context;
using Database.Interfaces;
using Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly PharmacyManagementContext _context;

        public UserRepository(PharmacyManagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetByIdAsync(Guid userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
        }

        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<User>> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Where(u => u.FirstName.Contains(username) || u.LastName.Contains(username))
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Where(u => u.Email.Contains(email))
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(string role)
        {
            return await _context.Users
                .Where(u => u.Role == role)
                .ToListAsync();
        }


    }
}