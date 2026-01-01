using System.ComponentModel.DataAnnotations;
using Business.DTO;

namespace MVC_WEB.Models.ViewModels
{
    /// <summary>
    /// ViewModel for the Purchases Index page
    /// </summary>
    public class PurchasesIndexViewModel
    {
        public IEnumerable<PurchaseOrderDTO> Orders { get; set; } = Enumerable.Empty<PurchaseOrderDTO>();
        
        // KPI Data
        public int TotalOrders { get; set; }
        public decimal TotalValue { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessedOrders { get; set; }
        public decimal OutstandingAmount { get; set; }
        
        // Filter properties
        public string? StatusFilter { get; set; }
        public string? DateFilter { get; set; }
        public string? SortBy { get; set; }
        public string? SearchQuery { get; set; }
    }

    /// <summary>
    /// ViewModel for creating a new Purchase Order
    /// </summary>
    public class PurchasesCreateViewModel
    {
        public IEnumerable<SupplierDTO> Suppliers { get; set; } = Enumerable.Empty<SupplierDTO>();
        public IEnumerable<ProductDTO> Products { get; set; } = Enumerable.Empty<ProductDTO>();
        
        [Required]
        public PurchaseOrderDTO Order { get; set; } = new PurchaseOrderDTO();
        
        // Item being added (temporary)
        public Guid? ProductId { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
    }

    /// <summary>
    /// ViewModel for editing a Purchase Order
    /// </summary>
    public class PurchasesEditViewModel
    {
        public IEnumerable<SupplierDTO> Suppliers { get; set; } = Enumerable.Empty<SupplierDTO>();
        public IEnumerable<ProductDTO> Products { get; set; } = Enumerable.Empty<ProductDTO>();
        
        [Required]
        public PurchaseOrderDTO Order { get; set; } = new PurchaseOrderDTO();
    }

    /// <summary>
    /// ViewModel for viewing Purchase Order details
    /// </summary>
    public class PurchasesDetailsViewModel
    {
        public PurchaseOrderDTO Order { get; set; } = new PurchaseOrderDTO();
        
        // Calculated properties
        public decimal PaidPercentage => Order.TotalAmount > 0 
            ? (Order.PaidAmount / Order.TotalAmount * 100) 
            : 0;
        
        public bool IsFullyPaid => Order.DueAmount <= 0;
        
        public string StatusClass => Order.Status?.ToLower() switch
        {
            "pending" => "bg-warning",
            "approved" => "bg-info",
            "processed" => "bg-success",
            "delivered" => "bg-success",
            "cancelled" => "bg-danger",
            _ => "bg-secondary"
        };
        
        public string StatusIcon => Order.Status?.ToLower() switch
        {
            "pending" => "fas fa-clock",
            "approved" => "fas fa-thumbs-up",
            "processed" => "fas fa-check-circle",
            "delivered" => "fas fa-truck",
            "cancelled" => "fas fa-times",
            _ => "fas fa-question"
        };
    }

    /// <summary>
    /// ViewModel for the cancel order modal
    /// </summary>
    public class CancelOrderViewModel
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Please provide a reason for cancellation")]
        public string Reason { get; set; } = string.Empty;
    }
}
