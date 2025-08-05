using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class ProductBatch
    {
        // Primary Key
        [Key]
        public Guid ProductBatchID { get; set; } = Guid.NewGuid();

        // Foreign Keys
        [Required]
        [ForeignKey(nameof(Product))]
        public string ProductID { get; set; } = string.Empty;

        [Required]
        [ForeignKey(nameof(Supplier))]
        public string SupplierID { get; set; } = string.Empty;

        // Navigation Properties
        public Product? Product { get; set; }
        public Supplier? Supplier { get; set; }

        // Value Properties
        [Required]
        public string BatchNumber { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddYears(2);

        [Required]
        public int QuantityInStock { get; set; } = 0;

        // Metadata
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}