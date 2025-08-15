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
            _context = context ?? throw new ArgumentNullException(nameof(context), "Context cannot be null");
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
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserID == user.UserID);
            if (existingUser == null)
            {
                throw new InvalidOperationException("User not found");
            }

            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            return await _context.Users
                .Where(u => EF.Functions.Like(u.FirstName, $"%{username}%") || EF.Functions.Like(u.LastName, $"%{username}%"))
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            }

            return await _context.Users
                .Where(u => EF.Functions.Like(u.Email, $"%{email}%"))
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException("Role cannot be null or empty", nameof(role));
            }

            return await _context.Users
                .Where(u => u.Role == role)
                .ToListAsync();
        }
    }
}