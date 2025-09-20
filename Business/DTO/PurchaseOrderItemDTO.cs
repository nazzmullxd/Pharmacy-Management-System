using System;
using System.ComponentModel.DataAnnotations;

namespace Business.DTO
{
    public class PurchaseOrderItemDTO
    {
        public Guid PurchaseOrderItemID { get; set; }
        public Guid PurchaseOrderID { get; set; }
        
        [Required(ErrorMessage = "Product is required")]
        public Guid ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int OrderedQuantity { get; set; }
        public int ReceivedQuantity { get; set; }
        public int PendingQuantity { get; set; }
        
        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = "Pending"; // "Pending", "Partial", "Received", "Cancelled"
    }
}
