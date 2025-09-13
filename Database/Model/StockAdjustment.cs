using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class StockAdjustment
    {
        [Key]
        public Guid StockAdjustmentID { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey(nameof(ProductBatch))]
        public Guid ProductBatchID { get; set; }

        public ProductBatch? ProductBatch { get; set; }

        [Required]
        public int PreviousQuantity { get; set; }

        [Required]
        public int AdjustedQuantity { get; set; }

        [Required]
        public int QuantityDifference { get; set; }

        [Required]
        public string AdjustmentType { get; set; } = string.Empty; // "Increase", "Decrease", "Correction"

        public string Reason { get; set; } = string.Empty;

        [Required]
        public Guid UserID { get; set; }

        public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow;

        public bool IsApproved { get; set; }

        public Guid? ApprovedBy { get; set; }

        public DateTime? ApprovalDate { get; set; }
    }
}