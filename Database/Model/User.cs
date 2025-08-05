using System;
using System.ComponentModel.DataAnnotations;

namespace Database.Model
{
    public enum UserRole
    {
        Admin,
        Employee
    }

    public class User
    {
        // Primary Key
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();

        // Value Properties
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        // Metadata
        [Required]
        public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;

        // Role
        [Required]
        [MaxLength(20)]
        public UserRole Role { get; set; } = UserRole.Employee;
    }
}