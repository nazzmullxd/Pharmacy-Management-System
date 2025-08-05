using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class Purchase
    {
        // Primary Key
        [Key]
        public string PurchaseID { get; set; } = Guid.NewGuid().ToString();

        // Foreign Keys
        [Required]
        [ForeignKey(nameof(Supplier))]
        public string SupplierID { get; set; } = string.Empty;

        [Required]
        [ForeignKey(nameof(User))]
        public string UserID { get; set; } = string.Empty;

        [ForeignKey(nameof(ProductBatch))]
        public string ProductBatchID { get; set; } = string.Empty;

        // Navigation Properties
        public Supplier? Supplier { get; set; }
        public User? User { get; set; }
        public ProductBatch? ProductBatch { get; set; }

        // Value Properties
        [Required]
        public decimal TotalAmount { get; set; } = 0.0m;

        // Metadata
        [Required]
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public string Notes { get; set; } = string.Empty;
    }
}