using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class ProductBatch
    {
        [Key]
        public string ProductBatchID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [ForeignKey(nameof(Product))]
        public string ProductID { get; set; } = string.Empty;
        public Product? Product { get; set; }

        [Required]
        public string BatchNumber { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddYears(2);

        [Required]
        public int QuantityInStock { get; set; } = 0;

        [Required]
        [ForeignKey(nameof(Supplier))]
        public string SupplierID { get; set; } = string.Empty;
        public Supplier? Supplier { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}