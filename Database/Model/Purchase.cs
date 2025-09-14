using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class Purchase
    {
        // Primary Key
        [Key]
        public Guid PurchaseID { get; set; } = Guid.NewGuid();

        // Foreign Keys
        [Required]
        [ForeignKey(nameof(Supplier))]
        public Guid SupplierID { get; set; } = Guid.Empty;

        [Required]
        [ForeignKey(nameof(UserInfo))]
        public Guid UserID { get; set; } = Guid.Empty;

        [ForeignKey(nameof(ProductBatch))]
        public Guid ProductBatchID { get; set; } = Guid.Empty;

        // Navigation Properties
        public Supplier? Supplier { get; set; }
        public UserInfo? User { get; set; }
        public ProductBatch? ProductBatch { get; set; }

        // Value Properties
        [Required]
        public decimal TotalAmount { get; set; } = 0.0m;
        // Value Properties
      

        [Required]
        public string PaymentStatus { get; set; } = "Pending";

        [Required]
        public decimal PaidAmount { get; set; } = 0.0m; // New property

        public decimal DueAmount { get; set; } = 0.0m; // New property

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // New property

        // Metadata
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public string Notes { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string OrderNumber { get; set; } = string.Empty;
    }
}