using Business.DTO;
using System.ComponentModel.DataAnnotations;

namespace MVC_WEB.Models.ViewModels
{
    /// <summary>
    /// View model for the Sales Index page with KPIs and filtering
    /// </summary>
    public class SalesIndexViewModel : BaseViewModel
    {
        public IEnumerable<SaleDTO> Sales { get; set; } = new List<SaleDTO>();
        
        // Summary KPIs
        public decimal TodaySales { get; set; }
        public int TodaySalesCount { get; set; }
        public decimal MonthlySales { get; set; }
        public int MonthlySalesCount { get; set; }
        public int PendingOrders { get; set; }
        public int TotalOrders { get; set; }
        
        // Filter properties
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        // View options
        public string ViewType { get; set; } = "table"; // table or card
        
        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalPages => (int)Math.Ceiling((double)TotalOrders / PageSize);
    }

    /// <summary>
    /// View model for creating a new sale
    /// </summary>
    public class SalesCreateViewModel : BaseViewModel
    {
        public SaleDTO Sale { get; set; } = new SaleDTO();
        
        // Reference data
        public IEnumerable<ProductDTO> Products { get; set; } = new List<ProductDTO>();
        public IEnumerable<CustomerDTO> Customers { get; set; } = new List<CustomerDTO>();
        
        // Form binding properties
        public Guid? SelectedCustomerID { get; set; }
        
        [Display(Name = "Product")]
        public Guid? ProductIdToAdd { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Quantity")]
        public int QuantityToAdd { get; set; } = 1;
        
        // Helper properties
        public string? CurrentUser { get; set; }
        public int TotalItems => Sale?.SaleItems?.Count ?? 0;
        public decimal Subtotal => Sale?.SaleItems?.Sum(x => x.Quantity * x.UnitPrice) ?? 0;
        public decimal TotalDiscount => Sale?.SaleItems?.Sum(x => x.Discount) ?? 0;
        
        // Payment status options
        public static List<SelectListItem> PaymentStatusOptions => new List<SelectListItem>
        {
            new SelectListItem { Value = "Paid", Text = "💳 Paid (Full Payment)" },
            new SelectListItem { Value = "Pending", Text = "⏳ Pending (Pay Later)" },
            new SelectListItem { Value = "Partial", Text = "💰 Partial Payment" }
        };
    }

    /// <summary>
    /// Select list item for dropdowns
    /// </summary>
    public class SelectListItem
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool Selected { get; set; }
    }

    /// <summary>
    /// View model for sale details page
    /// </summary>
    public class SalesDetailsViewModel : BaseViewModel
    {
        public SaleDTO? Sale { get; set; }
        
        // Computed properties
        public decimal Subtotal => Sale?.SaleItems?.Sum(x => x.Quantity * x.UnitPrice) ?? 0;
        public decimal TotalDiscount => Sale?.SaleItems?.Sum(x => x.Discount) ?? 0;
        public int TotalQuantity => Sale?.SaleItems?.Sum(x => x.Quantity) ?? 0;
        
        // Permission check
        public bool CanMarkAsPaid { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    /// <summary>
    /// View model for invoice page (standalone print layout)
    /// </summary>
    public class SalesInvoiceViewModel
    {
        public SaleDTO? Sale { get; set; }
        
        // Company information for invoice
        public string CompanyName { get; set; } = "Rabiul Pharmacy";
        public string CompanyAddress { get; set; } = "123 Healthcare Avenue\nMedical District, Health City 12345";
        public string CompanyPhone { get; set; } = "(555) 123-4567";
        public string CompanyEmail { get; set; } = "info@rabiulpharmacy.com";
        
        // Computed properties
        public decimal Subtotal => Sale?.SaleItems?.Sum(x => x.Quantity * x.UnitPrice) ?? 0;
        public decimal TotalDiscount => Sale?.SaleItems?.Sum(x => x.Discount) ?? 0;
        public string InvoiceNumber => Sale?.SaleID.ToString().Substring(0, 8).ToUpper() ?? "N/A";
    }

    /// <summary>
    /// View model for AJAX product stock validation
    /// </summary>
    public class ProductStockResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// View model for AJAX customer search
    /// </summary>
    public class CustomerSearchResult
    {
        public bool Success { get; set; }
        public IEnumerable<CustomerSearchItem> Customers { get; set; } = new List<CustomerSearchItem>();
    }

    public class CustomerSearchItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Contact { get; set; }
        public string? Email { get; set; }
    }

    /// <summary>
    /// View model for AJAX sale validation
    /// </summary>
    public class SaleValidationResult
    {
        public bool Success { get; set; }
        public bool CanProcess { get; set; }
        public decimal Total { get; set; }
        public string? Message { get; set; }
        public IEnumerable<StockValidationItem> Validation { get; set; } = new List<StockValidationItem>();
    }

    public class StockValidationItem
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Requested { get; set; }
        public int Available { get; set; }
        public bool IsValid { get; set; }
    }

    /// <summary>
    /// DTO for adding items to cart via AJAX
    /// </summary>
    public class AddItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    /// <summary>
    /// DTO for cart item operations
    /// </summary>
    public class CartItemResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public SaleItemDTO? Item { get; set; }
        public decimal NewTotal { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalDiscount { get; set; }
    }
}
