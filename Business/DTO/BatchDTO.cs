using System;

namespace Business.DTO
{
    public class BatchDTO
    {
        public Guid BatchID { get; set; }
        public Guid ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int QuantityInStock { get; set; }
    }
}
