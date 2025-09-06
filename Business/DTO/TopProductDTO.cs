using System;

namespace Business.DTO
{
    public class TopProductDTO
    {
        public Guid ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int Rank { get; set; }
    }
}
