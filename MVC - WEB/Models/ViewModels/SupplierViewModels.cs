using Business.DTO;
using System.ComponentModel.DataAnnotations;

namespace MVC_WEB.Models.ViewModels
{
    /// <summary>
    /// ViewModel for the Suppliers listing page
    /// </summary>
    public class SupplierListViewModel
    {
        public List<SupplierDTO> Suppliers { get; set; } = new List<SupplierDTO>();
        
        // Summary statistics
        public int TotalSuppliers { get; set; }
        public int ActiveSuppliers { get; set; }
        public int InactiveSuppliers { get; set; }
        public int CompleteInfoCount { get; set; }
        
        // Search/Filter
        public string? SearchQuery { get; set; }
        public string? StatusFilter { get; set; }
        public string? SortBy { get; set; }
    }

    /// <summary>
    /// ViewModel for viewing supplier details
    /// </summary>
    public class SupplierDetailsViewModel
    {
        public SupplierDTO Supplier { get; set; } = new SupplierDTO();
    }

    /// <summary>
    /// ViewModel for creating a new supplier
    /// </summary>
    public class SupplierCreateViewModel
    {
        [Required(ErrorMessage = "Supplier name is required")]
        [StringLength(100, ErrorMessage = "Supplier name cannot exceed 100 characters")]
        [Display(Name = "Supplier Name")]
        public string SupplierName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Contact person name cannot exceed 100 characters")]
        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// ViewModel for editing an existing supplier
    /// </summary>
    public class SupplierEditViewModel
    {
        public Guid SupplierID { get; set; }

        [Required(ErrorMessage = "Supplier name is required")]
        [StringLength(100, ErrorMessage = "Supplier name cannot exceed 100 characters")]
        [Display(Name = "Supplier Name")]
        public string SupplierName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Contact person name cannot exceed 100 characters")]
        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Read-only properties
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// ViewModel for the delete confirmation page
    /// </summary>
    public class SupplierDeleteViewModel
    {
        public SupplierDTO Supplier { get; set; } = new SupplierDTO();
    }
}
