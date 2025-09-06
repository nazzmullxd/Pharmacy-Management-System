using System;
using System.Collections.Generic;

namespace Business.DTO
{
    public class ReportDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal NetProfit { get; set; }
        public int TotalTransactions { get; set; }
        public List<TopProductDTO> TopSellingProducts { get; set; } = new List<TopProductDTO>();
        public List<SaleDTO> RecentSales { get; set; } = new List<SaleDTO>();
        public List<ExpiryAlertDTO> ExpiringProducts { get; set; } = new List<ExpiryAlertDTO>();
    }
}
