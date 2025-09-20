using System;

namespace Business.DTO
{
    public class SaleItemDTO
    {
        public Guid SaleItemID { get; set; }
        public Guid SaleID { get; set; }
        public Guid ProductID { get; set; }
        public Guid? ProductBatchID { get; set; } = null;
        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Newly surfaced discount (was ignored previously)
        public decimal Discount { get; set; }

        // Keep TotalPrice for backward compatibility but ensure it is always synchronized.
        public decimal TotalPrice { get; set; }

        public string BatchNumber { get; set; } = string.Empty;

        // Helper to force recompute (not serialized automatically unless called explicitly)
        public void RecomputeTotal() => TotalPrice = (UnitPrice * Quantity) - Discount;
    }
}
