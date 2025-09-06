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

            var user = await _userRepository.GetByEmailAsync(email);
            return user != null ? MapToDTO(user) : null;
        }

        public async Task<UserDTO> CreateUserAsync(UserDTO userDto, string password)
        {
            if (userDto == null)
                throw new ArgumentNullException(nameof(userDto));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required", nameof(password));

            if (string.IsNullOrWhiteSpace(userDto.Email))
                throw new ArgumentException("Email is required", nameof(userDto));

            if (string.IsNullOrWhiteSpace(userDto.FirstName))
                throw new ArgumentException("First name is required", nameof(userDto));

            if (string.IsNullOrWhiteSpace(userDto.LastName))
                throw new ArgumentException("Last name is required", nameof(userDto));

            if (string.IsNullOrWhiteSpace(userDto.Role))
                throw new ArgumentException("Role is required", nameof(userDto));

            // Check if email already exists
            var existingUser = await _userRepository.GetByEmailAsync(userDto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("User with this email already exists");

            var user = MapToEntity(userDto);
            user.PasswordHash = HashPassword(password);
            user.CreatedDate = DateTime.UtcNow;
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
                throw new InvalidOperationException("User not found");

            // Update properties
            existingUser.FirstName = userDto.FirstName;
            existingUser.LastName = userDto.LastName;
            existingUser.Email = userDto.Email;
            existingUser.Role = userDto.Role;
            existingUser.UpdatedDate = DateTime.UtcNow;

            await _userRepository.UpdateAsync(existingUser);
            return MapToDTO(existingUser);
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            await _userRepository.DeleteAsync(userId);
            return true;
        }

        public async Task<bool> AuthenticateUserAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return false;

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return false;

            // Update last login date
            user.LastLoginDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("New password cannot be null or empty", nameof(newPassword));

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            if (!VerifyPassword(currentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = HashPassword(newPassword);
            user.UpdatedDate = DateTime.UtcNow;
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
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.LastLoginDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;

            var user = await _userRepository.GetByEmailAsync(email);
            return user == null || user.UserID == excludeUserId;
        }

        private UserDTO MapToDTO(UserInfo user)
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

        private UserInfo MapToEntity(UserDTO userDto)
        {
            return new UserInfo
            {
                UserID = userDto.UserID,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Role = userDto.Role,
                LastLoginDate = userDto.LastLoginDate
            };
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }
}