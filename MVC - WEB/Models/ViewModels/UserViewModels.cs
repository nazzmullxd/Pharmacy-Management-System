using Business.DTO;
using System.ComponentModel.DataAnnotations;

namespace MVC_WEB.Models.ViewModels
{
    /// <summary>
    /// ViewModel for the Users listing page
    /// </summary>
    public class UserListViewModel
    {
        public List<UserDTO> Users { get; set; } = new List<UserDTO>();
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int AdminCount { get; set; }
        public int StaffCount { get; set; }
    }

    /// <summary>
    /// ViewModel for viewing user details
    /// </summary>
    public class UserDetailsViewModel
    {
        public UserDTO User { get; set; } = new UserDTO();
    }

    /// <summary>
    /// ViewModel for creating a new user
    /// </summary>
    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm the password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Role")]
        public string Role { get; set; } = "User";

        public List<string> AvailableRoles { get; set; } = new List<string>();
    }

    /// <summary>
    /// ViewModel for editing an existing user
    /// </summary>
    public class UserEditViewModel
    {
        public Guid UserID { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Role")]
        public string Role { get; set; } = "User";

        public List<string> AvailableRoles { get; set; } = new List<string>();
    }

    /// <summary>
    /// ViewModel for the delete confirmation page
    /// </summary>
    public class UserDeleteViewModel
    {
        public UserDTO User { get; set; } = new UserDTO();
    }

    /// <summary>
    /// ViewModel for changing user password
    /// </summary>
    public class UserChangePasswordViewModel
    {
        public Guid UserID { get; set; }
        
        [Display(Name = "User")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm the new password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
