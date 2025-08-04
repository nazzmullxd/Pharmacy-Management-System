using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class PurchaseItem
    {
        [Key]
        public string PurchaseItemID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [ForeignKey(nameof(Purchase))]
        public string PurchaseID { get; set; } = string.Empty;
        public Purchase? Purchase { get; set; }

        [Required]
        [ForeignKey(nameof(ProductBatch))]
        public string ProductBatchID { get; set; } = string.Empty;
        public ProductBatch? ProductBatch { get; set; }

        [Required]
        [ForeignKey(nameof(Product))]
        public string ProductID { get; set; } = string.Empty;
        public Product? Product { get; set; }

        [Required]
        public int Quantity { get; set; } = 0;

        [Required]
        public decimal UnitPrice { get; set; } = 0.0m;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}