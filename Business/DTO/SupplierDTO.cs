using System;
using System.ComponentModel.DataAnnotations;

namespace Business.DTO
{
    public class SupplierDTO
    {
        public Guid SupplierID { get; set; }
        
        [Required(ErrorMessage = "Supplier name is required")]
        [StringLength(100, ErrorMessage = "Supplier name cannot exceed 100 characters")]
        public string SupplierName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Contact person is required")]
        [StringLength(100, ErrorMessage = "Contact person name cannot exceed 100 characters")]
        public string ContactPerson { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
