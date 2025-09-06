using System;

namespace Business.DTO
{
    public class ExpiryAlertDTO
    {
        public Guid ProductBatchID { get; set; }
        public Guid ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int QuantityInStock { get; set; }
        public int DaysUntilExpiry { get; set; }
        public string AlertLevel { get; set; } = string.Empty; // "Critical", "Warning", "Info"
        public string SupplierName { get; set; } = string.Empty;
    }
}
