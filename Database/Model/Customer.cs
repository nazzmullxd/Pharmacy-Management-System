using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class Customer
    {
        // Primary Key
        [Key]
        public Guid CustomerID { get; set; } = Guid.NewGuid();

        // Value Properties
        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        public string ContactNumber { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        // Metadata
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}