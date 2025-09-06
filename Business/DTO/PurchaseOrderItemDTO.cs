using System;

namespace Business.DTO
{
    public class PurchaseOrderItemDTO
    {
        public Guid PurchaseOrderItemID { get; set; }
        public Guid PurchaseOrderID { get; set; }
        public Guid ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int OrderedQuantity { get; set; }
        public int ReceivedQuantity { get; set; }
        public int PendingQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = "Pending"; // "Pending", "Partial", "Received", "Cancelled"
    }
}
