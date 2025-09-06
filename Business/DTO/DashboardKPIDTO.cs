using System;

namespace Business.DTO
{
    public class DashboardKPIDTO
    {
        public DateTime Date { get; set; }
        
        // Today's Metrics
        public int TodaySaleOrdersCount { get; set; }
        public int TodayInvoicesCount { get; set; }
        public decimal TodaySalesAmount { get; set; }
        public decimal TodayInvoiceAmount { get; set; }
        
        // This Month's Metrics
        public int ThisMonthSaleOrdersCount { get; set; }
        public int ThisMonthInvoicesCount { get; set; }
        public decimal ThisMonthSalesAmount { get; set; }
        public decimal ThisMonthInvoiceAmount { get; set; }
        
        // Stock Metrics
        public int StockItemsCount { get; set; }
        public int StockAdjustmentsCount { get; set; }
        public decimal TotalStockValue { get; set; }
        public int LowStockItemsCount { get; set; }
        public int ExpiringItemsCount { get; set; }
        
        // Financial Metrics
        public decimal SalesDue { get; set; }
        public decimal InvoiceDue { get; set; }
        public decimal TotalDue { get; set; }
        public decimal NetProfit { get; set; }
        
        // Support Metrics
        public int SupportTicketsCount { get; set; }
        public int OpenSupportTicketsCount { get; set; }
        
        // Performance Indicators
        public decimal SalesGrowthPercentage { get; set; }
        public decimal ProfitMarginPercentage { get; set; }
        public int TopSellingProductsCount { get; set; }
    }
}
