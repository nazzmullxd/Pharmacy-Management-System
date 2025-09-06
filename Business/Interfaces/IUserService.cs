using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Business.DTO;
using Database.Model;

namespace Business.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO?> GetUserByIdAsync(Guid userId);
        Task<UserDTO?> GetUserByEmailAsync(string email);
        Task<UserDTO> CreateUserAsync(UserDTO userDto, string password);
        Task<UserDTO> UpdateUserAsync(UserDTO userDto);
        Task<bool> DeleteUserAsync(Guid userId);
        Task<bool> AuthenticateUserAsync(string email, string password);
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
        Task<IEnumerable<UserDTO>> GetUsersByRoleAsync(string role);
        Task<bool> UpdateLastLoginAsync(Guid userId);
        Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null);
    }
}
