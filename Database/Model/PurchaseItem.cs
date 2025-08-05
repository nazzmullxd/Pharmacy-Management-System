using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class PurchaseItem
    {
        // Primary Key
        [Key]
        public Guid PurchaseItemID { get; set; } = Guid.NewGuid();

        // Foreign Keys
        [Required]
        [ForeignKey(nameof(Purchase))]
        public Guid PurchaseID { get; set; } = Guid.Empty;

        [Required]
        [ForeignKey(nameof(ProductBatch))]
        public Guid ProductBatchID { get; set; } = Guid.Empty;

        [Required]
        [ForeignKey(nameof(Product))]
        public Guid ProductID { get; set; } = Guid.Empty;

        // Navigation Properties
        public Purchase? Purchase { get; set; }
        public ProductBatch? ProductBatch { get; set; }
        public Product? Product { get; set; }

        // Value Properties
        [Required]
        public int Quantity { get; set; } = 0;

        [Required]
        public decimal UnitPrice { get; set; } = 0.0m;

        // Metadata
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}