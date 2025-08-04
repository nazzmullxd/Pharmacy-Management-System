using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class Purchase
    {
        [Key]
        public string PurchaseID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [ForeignKey(nameof(Supplier))]
        public string SupplierID { get; set; } = string.Empty;
        public Supplier? Supplier { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public string UserID { get; set; } = string.Empty;
        public User? User { get; set; }

        [ForeignKey(nameof(ProductBatch))]
        public string ProductBatchID { get; set; } = string.Empty;
        public ProductBatch? ProductBatch { get; set; }

        [Required]
        public decimal TotalAmount { get; set; } = 0.0m;

        [Required]
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public string Notes { get; set; } = string.Empty;
    }
}