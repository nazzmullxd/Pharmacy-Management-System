using Business.DTO;
using System.ComponentModel.DataAnnotations;

namespace MVC_WEB.Models.ViewModels
{
    // Support ViewModels
    public class SupportIndexViewModel
    {
        public IEnumerable<SupportTicketDTO> Tickets { get; set; } = new List<SupportTicketDTO>();
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int ClosedTickets { get; set; }
        public string? StatusFilter { get; set; }
    }

    public class SupportCreateViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Priority is required")]
        public string Priority { get; set; } = "Normal";
    }

    public class SupportDetailsViewModel
    {
        public SupportTicketDTO Ticket { get; set; } = new SupportTicketDTO();
    }

    // Profile ViewModels
    public class ProfileViewModel
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProfileEditViewModel
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // Settings ViewModels
    public class SettingsViewModel
    {
        public GeneralSettings General { get; set; } = new GeneralSettings();
        public NotificationSettings Notifications { get; set; } = new NotificationSettings();
        public DisplaySettings Display { get; set; } = new DisplaySettings();
    }

    public class GeneralSettings
    {
        public string Language { get; set; } = "en";
        public string Timezone { get; set; } = "UTC";
        public string DateFormat { get; set; } = "MM/dd/yyyy";
        public string Currency { get; set; } = "USD";
    }

    public class NotificationSettings
    {
        public bool EmailNotifications { get; set; } = true;
        public bool LowStockAlerts { get; set; } = true;
        public bool ExpiryAlerts { get; set; } = true;
        public bool SaleNotifications { get; set; } = false;
    }

    public class DisplaySettings
    {
        public string Theme { get; set; } = "light";
        public int ItemsPerPage { get; set; } = 10;
        public bool ShowDashboardStats { get; set; } = true;
    }

    // Pharmacy ViewModels
    public class PharmacyInfoViewModel
    {
        public string PharmacyName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public DateTime? LicenseExpiry { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class PharmacySettingsViewModel
    {
        [Required(ErrorMessage = "Pharmacy name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string PharmacyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "License number is required")]
        public string LicenseNumber { get; set; } = string.Empty;

        public string? OwnerName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LicenseExpiry { get; set; }

        // Business Settings
        public decimal TaxRate { get; set; } = 0;
        public int LowStockThreshold { get; set; } = 10;
        public int ExpiryAlertDays { get; set; } = 30;
        public bool RequirePrescriptionForAntibiotics { get; set; } = true;
        public string InvoicePrefix { get; set; } = "INV";
        public string ReceiptFooter { get; set; } = string.Empty;
    }
}
