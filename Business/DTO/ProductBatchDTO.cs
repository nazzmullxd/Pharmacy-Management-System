using System;

namespace Business.DTO
{
    public class ProductBatchDTO
    {
        public Guid ProductBatchID { get; set; }
        public Guid ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid SupplierID { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int QuantityInStock { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsExpired => ExpiryDate <= DateTime.UtcNow;
        public bool IsExpiringSoon => ExpiryDate <= DateTime.UtcNow.AddDays(30);
    }
}
