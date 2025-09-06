using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Business.DTO;
using Business.Interfaces;
using Database.Interfaces;
using Database.Model;

namespace Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<IEnumerable<UserInfo>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<UserInfo?> GetUserByIdAsync(Guid userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task AddUserAsync(UserInfo user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }

            await _userRepository.AddAsync(user);
        }

        public async Task UpdateUserAsync(UserInfo user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }

            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            await _userRepository.DeleteAsync(userId);
        }

        public async Task<IEnumerable<UserInfo>> SearchUsersByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<IEnumerable<UserInfo>> SearchUsersByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            }

            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<UserInfo>> GetUsersByRoleAsync(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException("Role cannot be null or empty", nameof(role));
            }

            return await _userRepository.GetByRoleAsync(role);
        }

        // IUserService implementation
        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDTO);
        }

        public async Task<UserDTO?> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user != null ? MapToDTO(user) : null;
        }

        public async Task<UserDTO?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            var users = await _userRepository.GetByEmailAsync(email);
            var user = users.FirstOrDefault();
            return user != null ? MapToDTO(user) : null;
        }

        public async Task<UserDTO> CreateUserAsync(UserDTO userDto, string password)
        {
            if (userDto == null)
                throw new ArgumentNullException(nameof(userDto));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            var user = MapToEntity(userDto);
            user.PasswordHash = HashPassword(password);
            user.UserID = Guid.NewGuid();
            user.LastLoginDate = DateTime.UtcNow;

            await _userRepository.AddAsync(user);
            return MapToDTO(user);
        }

        public async Task<UserDTO> UpdateUserAsync(UserDTO userDto)
        {
            if (userDto == null)
                throw new ArgumentNullException(nameof(userDto));

            var existingUser = await _userRepository.GetByIdAsync(userDto.UserID);
            if (existingUser == null)
                throw new ArgumentException("User not found", nameof(userDto.UserID));

            var user = MapToEntity(userDto);
            user.PasswordHash = existingUser.PasswordHash; // Preserve existing password

            await _userRepository.UpdateAsync(user);
            return MapToDTO(user);
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            try
            {
                await _userRepository.DeleteAsync(userId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AuthenticateUserAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return false;

            var users = await _userRepository.GetByEmailAsync(email);
            var user = users.FirstOrDefault();
            
            if (user == null)
                return false;

            return VerifyPassword(password, user.PasswordHash);
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
                return false;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !VerifyPassword(currentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<IEnumerable<UserDTO>> GetUsersByRoleAsync(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role cannot be null or empty", nameof(role));

            var users = await _userRepository.GetByRoleAsync(role);
            return users.Select(MapToDTO);
        }

        public async Task<bool> UpdateLastLoginAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                user.LastLoginDate = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var users = await _userRepository.GetByEmailAsync(email);
            return !users.Any(u => u.UserID != excludeUserId);
        }

        // Helper methods
        private static UserDTO MapToDTO(UserInfo user)
        {
            return new UserDTO
            {
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                LastLoginDate = user.LastLoginDate
            };
        }

        private static UserInfo MapToEntity(UserDTO userDto)
        {
            return new UserInfo
            {
                UserID = userDto.UserID,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Role = userDto.Role,
                LastLoginDate = userDto.LastLoginDate,
                PasswordHash = string.Empty // Will be set separately
            };
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}