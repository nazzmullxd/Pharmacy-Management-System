using System;

namespace Business.DTO
{
    public class UserDTO
    {
        public Guid UserID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime LastLoginDate { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}
