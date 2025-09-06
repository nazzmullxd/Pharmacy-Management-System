using System;

namespace Business.DTO
{
    public class PurchaseItemDTO
    {
        public Guid PurchaseItemID { get; set; }
        public Guid PurchaseID { get; set; }
        public Guid ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
    }
}
