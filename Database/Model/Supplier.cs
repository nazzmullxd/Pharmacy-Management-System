using System;
using System.ComponentModel.DataAnnotations;

namespace Database.Model
{
    public enum CreatedBy
    {
        Admin,
        Employee
    }

    public class Supplier
    {
        [Key]
        public string SupplierID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string CompanyName { get; set; } = string.Empty;

        public string ContactPerson { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        public CreatedBy CreatedBy { get; set; } = CreatedBy.Admin;

        public bool IsActive { get; set; } = true;
    }
}