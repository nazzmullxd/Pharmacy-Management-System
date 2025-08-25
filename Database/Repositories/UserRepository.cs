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

        public async Task<IEnumerable<UserInfo>> GetAllAsync()
        {
            return await _context.UsersInfo.ToListAsync(); // Updated from Users to UsersInfo
        }

        public async Task<UserInfo?> GetByIdAsync(Guid userId)
        {
            return await _context.UsersInfo.FirstOrDefaultAsync(u => u.UserID == userId); // Updated from Users to UsersInfo
        }

        public async Task AddAsync(UserInfo user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }

            await _context.UsersInfo.AddAsync(user); // Updated from Users to UsersInfo
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserInfo user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }

            var existingUser = await _context.UsersInfo.FirstOrDefaultAsync(u => u.UserID == user.UserID); // Updated from Users to UsersInfo
            if (existingUser == null)
            {
                throw new InvalidOperationException("User not found");
            }

            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid userId)
        {
            var user = await _context.UsersInfo.FirstOrDefaultAsync(u => u.UserID == userId); // Updated from Users to UsersInfo
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            _context.UsersInfo.Remove(user); // Updated from Users to UsersInfo
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserInfo>> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            return await _context.UsersInfo // Updated from Users to UsersInfo
                .Where(u => EF.Functions.Like(u.FirstName, $"%{username}%") || EF.Functions.Like(u.LastName, $"%{username}%"))
                .ToListAsync();
        }

        public async Task<IEnumerable<UserInfo>> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            }

            return await _context.UsersInfo // Updated from Users to UsersInfo
                .Where(u => EF.Functions.Like(u.Email, $"%{email}%"))
                .ToListAsync();
        }

        public async Task<IEnumerable<UserInfo>> GetByRoleAsync(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException("Role cannot be null or empty", nameof(role));
            }

            return await _context.UsersInfo // Updated from Users to UsersInfo
                .Where(u => u.Role == role)
                .ToListAsync();
        }
    }
}