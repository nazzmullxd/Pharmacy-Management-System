using System;

namespace Business.DTO
{
    public class StockAdjustmentDTO
    {
        public Guid StockAdjustmentID { get; set; }
        public Guid ProductBatchID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public int PreviousQuantity { get; set; }
        public int AdjustedQuantity { get; set; }
        public int QuantityDifference { get; set; }
        public string AdjustmentType { get; set; } = string.Empty; // "Increase", "Decrease", "Correction"
        public string Reason { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public Guid UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime AdjustmentDate { get; set; }
        public bool IsApproved { get; set; }
        public Guid? ApprovedBy { get; set; }
        public string ApprovedByName { get; set; } = string.Empty;
        public DateTime? ApprovalDate { get; set; }
    }
}
