using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class SaleItem
    {
        // Primary Key
        [Key]
        public Guid SalesItemID { get; set; } = Guid.NewGuid();

        // Foreign Keys
        [Required]
        [ForeignKey(nameof(Sale))]
        public string SaleID { get; set; } = string.Empty;

        [Required]
        [ForeignKey(nameof(ProductBatch))]
        public string ProductBatchID { get; set; } = string.Empty;

        [Required]
        [ForeignKey(nameof(Product))]
        public string ProductID { get; set; } = string.Empty;

        // Navigation Properties
        public Sale? Sale { get; set; }
        public ProductBatch? ProductBatch { get; set; }
        public Product? Product { get; set; }

        // Value Properties
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; } = 0;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0.")]
        public decimal UnitPrice { get; set; } = 0.0m;

        public decimal Discount { get; set; } = 0.0m;

        // Metadata
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}