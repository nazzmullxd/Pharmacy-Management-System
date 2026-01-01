using Business.DTO;

namespace MVC_WEB.Models.ViewModels
{
    /// <summary>
    /// View model for the Reports Index/Dashboard page
    /// </summary>
    public class ReportsIndexViewModel : BaseViewModel
    {
        // Quick stats for the dashboard
        public decimal? TodaysSales { get; set; }
        public int? LowStockItems { get; set; }
        public int? ExpiringItems { get; set; }
        public int? TotalCustomers { get; set; }
    }

    /// <summary>
    /// View model for the Sales Reports page
    /// </summary>
    public class SalesReportViewModel : BaseViewModel
    {
        public IEnumerable<SaleDTO> Sales { get; set; } = new List<SaleDTO>();
        public IEnumerable<TopProductDTO> TopProducts { get; set; } = new List<TopProductDTO>();
        
        // Summary metrics
        public decimal TotalSales { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TodaySales { get; set; }
        public decimal MonthlySales { get; set; }
        
        // Computed properties
        public decimal AverageSale => TotalInvoices > 0 ? TotalSales / TotalInvoices : 0;
        public decimal DailyAverage => MonthlySales / Math.Max(DateTime.Today.Day, 1);
        
        // Filter properties
        public string? ReportType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// View model for the Stock Reports page
    /// </summary>
    public class StockReportViewModel : BaseViewModel
    {
        public IEnumerable<ProductDTO> Products { get; set; } = new List<ProductDTO>();
        public IEnumerable<ProductBatchDTO> Batches { get; set; } = new List<ProductBatchDTO>();
        public IEnumerable<ExpiryAlertDTO> ExpiringItems { get; set; } = new List<ExpiryAlertDTO>();
        
        // Summary metrics
        public decimal TotalStockValue { get; set; }
        public int LowStockCount { get; set; }
        public int TotalProducts => Products.Count();
        public int TotalBatches => Batches.Count();
        
        // Computed properties
        public double NormalStockPercentage => TotalProducts > 0 ? 
            ((TotalProducts - LowStockCount) * 100.0 / TotalProducts) : 0;
        public double LowStockPercentage => TotalProducts > 0 ? 
            (LowStockCount * 100.0 / TotalProducts) : 0;
        public double ExpiringPercentage => TotalBatches > 0 ? 
            (ExpiringItems.Count() * 100.0 / TotalBatches) : 0;
        
        // Filter properties
        public string? StockFilter { get; set; }
        public string? ExpiryFilter { get; set; }
    }

    /// <summary>
    /// View model for the Profit & Loss Report page
    /// </summary>
    public class ProfitLossReportViewModel : BaseViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public IEnumerable<SaleDTO> Sales { get; set; } = new List<SaleDTO>();
        
        // Computed properties
        public decimal CostPercentage => TotalRevenue > 0 ? (TotalCost / TotalRevenue * 100) : 0;
        public string ProfitStatus => ProfitMargin >= 20 ? "Excellent" : ProfitMargin >= 10 ? "Good" : "Needs Improvement";
        public string ProfitStatusColor => ProfitMargin >= 20 ? "success" : ProfitMargin >= 10 ? "warning" : "danger";
        
        // Filter properties
        public string? PeriodFilter { get; set; }
        public string? ReportType { get; set; }
    }

    /// <summary>
    /// View model for the Antibiotic Register page
    /// </summary>
    public class AntibioticReportViewModel : BaseViewModel
    {
        public IEnumerable<AntibioticLogDTO> AntibioticLogs { get; set; } = new List<AntibioticLogDTO>();
        
        // Summary metrics
        public int TotalRecords => AntibioticLogs.Count();
        public int TodaysPrescriptions => AntibioticLogs.Where(x => x.SaleDate.Date == DateTime.Today).Count();
        public int UniqueCustomers => AntibioticLogs.Select(x => x.CustomerName).Distinct().Count();
        
        // Compliance rate
        public double ComplianceRate => TotalRecords > 0 ? 
            (AntibioticLogs.Where(x => !string.IsNullOrEmpty(x.PrescriptionNumber)).Count() * 100.0 / TotalRecords) : 0;
        public string ComplianceStatus => ComplianceRate >= 95 ? "Excellent" : ComplianceRate >= 80 ? "Good" : "Needs Attention";
        public string ComplianceColor => ComplianceRate >= 95 ? "success" : ComplianceRate >= 80 ? "warning" : "danger";
        
        // Filter properties
        public string? DateFilter { get; set; }
        public string? ProductFilter { get; set; }
    }
}
