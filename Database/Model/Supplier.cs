using System;
using System.ComponentModel.DataAnnotations;

namespace Database.Model
{
    /*
    public enum CreatedBy
    {
        Admin,
        Employee
    }
    */

    public class Supplier
    {
        // Primary Key
        [Key]
        public Guid SupplierID { get; set; } = Guid.NewGuid();

        // Value Properties
        [Required]
        public string SupplierName { get; set; } = string.Empty; // Supplier Company Name

        [Required]
        public string ContactPerson { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        // Metadata
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;

        // Status
        public bool IsActive { get; set; } = true;
    }
}