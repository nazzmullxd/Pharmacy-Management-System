using System;

namespace Business.DTO
{
    public class StockAdjustmentDTO
    {
        public Guid StockAdjustmentID { get; set; }
        public Guid ProductBatchID { get; set; }
        public int PreviousQuantity { get; set; }
        public int AdjustedQuantity { get; set; }
        public int QuantityDifference { get; set; }
        public string AdjustmentType { get; set; } = string.Empty; // "increase", "decrease", "correction", "transfer"
        public string Reason { get; set; } = string.Empty;
        public Guid UserID { get; set; }
        public DateTime AdjustmentDate { get; set; }
        public bool IsApproved { get; set; }
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
    }

    public class StockAdjustmentResultDTO
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}