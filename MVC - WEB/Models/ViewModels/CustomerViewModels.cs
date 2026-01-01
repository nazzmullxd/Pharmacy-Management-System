using Business.DTO;
using System.ComponentModel.DataAnnotations;

namespace MVC_WEB.Models.ViewModels
{
    /// <summary>
    /// ViewModel for the Customers listing page
    /// </summary>
    public class CustomerListViewModel
    {
        public List<CustomerDTO> Customers { get; set; } = new List<CustomerDTO>();
        public List<CustomerDTO> RecentCustomers { get; set; } = new List<CustomerDTO>();
        public List<CustomerDTO> TopCustomers { get; set; } = new List<CustomerDTO>();
        
        // Summary statistics
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int NewCustomers { get; set; }
        public decimal TotalCustomerSales { get; set; }
        
        // Search/Filter
        public string? SearchQuery { get; set; }
        public string? StatusFilter { get; set; }
        public string? SortBy { get; set; }
    }

    /// <summary>
    /// ViewModel for viewing customer details
    /// </summary>
    public class CustomerDetailsViewModel
    {
        public CustomerDTO Customer { get; set; } = new CustomerDTO();
        public List<SaleDTO>? RecentSales { get; set; }
    }

    /// <summary>
    /// ViewModel for creating a new customer
    /// </summary>
    public class CustomerCreateViewModel
    {
        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(200, ErrorMessage = "Customer name cannot exceed 200 characters")]
        [Display(Name = "Full Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required")]
        [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [Display(Name = "Address")]
        public string? Address { get; set; }
    }

    /// <summary>
    /// ViewModel for editing an existing customer
    /// </summary>
    public class CustomerEditViewModel
    {
        public Guid CustomerID { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(200, ErrorMessage = "Customer name cannot exceed 200 characters")]
        [Display(Name = "Full Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required")]
        [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        // Read-only display properties
        public DateTime CreatedDate { get; set; }
        public decimal? TotalPurchases { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
    }

    /// <summary>
    /// ViewModel for the delete confirmation page
    /// </summary>
    public class CustomerDeleteViewModel
    {
        public CustomerDTO Customer { get; set; } = new CustomerDTO();
    }
}
