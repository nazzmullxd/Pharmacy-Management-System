using System.ComponentModel.DataAnnotations;
using Business.DTO;

namespace MVC_WEB.Models.ViewModels
{
    // Stock Index ViewModel - Main stock overview
    public class StockIndexViewModel
    {
        public IEnumerable<ProductDTO> StockItems { get; set; } = new List<ProductDTO>();
        public IEnumerable<ProductDTO> LowStockAlerts { get; set; } = new List<ProductDTO>();
        public IEnumerable<ExpiryAlertDTO> ExpiringAlerts { get; set; } = new List<ExpiryAlertDTO>();
        
        // Summary statistics
        public int TotalProducts { get; set; }
        public int LowStockItems { get; set; }
        public int ExpiringItems { get; set; }
        public decimal TotalStockValue { get; set; }
    }

    // Stock Batches ViewModel
    public class StockBatchesViewModel
    {
        public IEnumerable<ProductBatchDTO> Batches { get; set; } = Enumerable.Empty<ProductBatchDTO>();
        public Guid? ProductIdFilter { get; set; }
        
        // Summary statistics
        public int TotalBatches { get; set; }
        public int ExpiringSoon { get; set; }
        public int LowStockBatches { get; set; }
        public int TotalUnits { get; set; }
    }

    // Stock Expiring ViewModel
    public class StockExpiringViewModel
    {
        public IEnumerable<ProductBatchDTO> Batches { get; set; } = Enumerable.Empty<ProductBatchDTO>();
        public int DaysAhead { get; set; } = 30;
        
        // Summary statistics
        public int ExpiredItems { get; set; }
        public int ExpiringThisWeek { get; set; }
        public int ExpiringThisMonth { get; set; }
        public decimal AtRiskValue { get; set; }
    }

    // Stock Adjustments ViewModel
    public class StockAdjustmentsViewModel
    {
        public IEnumerable<ProductBatchDTO> Batches { get; set; } = Enumerable.Empty<ProductBatchDTO>();
        public IEnumerable<StockAdjustmentDTO> RecentAdjustments { get; set; } = new List<StockAdjustmentDTO>();
        
        // Form fields
        public Guid? SelectedBatchId { get; set; }
        public string AdjustmentType { get; set; } = "Correction";
        public int AdjustedQuantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        
        public Guid? ProductIdFilter { get; set; }
        public string? Message { get; set; }
    }

    // Stock Adjustment Create ViewModel (for form submission)
    public class StockAdjustmentCreateViewModel
    {
        [Required(ErrorMessage = "Please select a batch")]
        public Guid SelectedBatchId { get; set; }

        [Required(ErrorMessage = "Adjustment type is required")]
        public string AdjustmentType { get; set; } = "Correction";

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int AdjustedQuantity { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; } = string.Empty;
    }

    // Stock Add Batch ViewModel
    public class StockAddBatchViewModel
    {
        public IEnumerable<ProductDTO> Products { get; set; } = Enumerable.Empty<ProductDTO>();
        public IEnumerable<SupplierDTO> Suppliers { get; set; } = Enumerable.Empty<SupplierDTO>();

        [Required(ErrorMessage = "Product is required")]
        public Guid ProductID { get; set; }

        [Required(ErrorMessage = "Supplier is required")]
        public Guid SupplierID { get; set; }

        [Required(ErrorMessage = "Batch number is required")]
        [StringLength(50, ErrorMessage = "Batch number cannot exceed 50 characters")]
        public string BatchNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiry date is required")]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; } = DateTime.Now.AddYears(1);

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int QuantityInStock { get; set; }
    }

    // Stock Details ViewModel
    public class StockDetailsViewModel
    {
        public ProductDTO Product { get; set; } = new ProductDTO();
        public IEnumerable<ProductBatchDTO> Batches { get; set; } = Enumerable.Empty<ProductBatchDTO>();
    }

    // Stock Low Stock ViewModel
    public class StockLowStockViewModel
    {
        public IEnumerable<ProductDTO> LowStockProducts { get; set; } = new List<ProductDTO>();
    }
}
