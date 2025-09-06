using System;
using System.Collections.Generic;

namespace Business.DTO
{
    public class PurchaseOrderDTO
    {
        public Guid PurchaseOrderID { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid SupplierID { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public Guid CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Ordered", "Delivered", "Cancelled"
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public string PaymentStatus { get; set; } = "Pending"; // "Pending", "Partial", "Paid"
        public string Notes { get; set; } = string.Empty;
        public List<PurchaseOrderItemDTO> OrderItems { get; set; } = new List<PurchaseOrderItemDTO>();
    }
}
