using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Purchases
{
    public class CreateModel : PageModel
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;

        public CreateModel(IPurchaseOrderService purchaseOrderService, ISupplierService supplierService, IProductService productService)
        {
            _purchaseOrderService = purchaseOrderService;
            _supplierService = supplierService;
            _productService = productService;
        }

        public IEnumerable<SupplierDTO> Suppliers { get; set; } = Enumerable.Empty<SupplierDTO>();
        public IEnumerable<ProductDTO> Products { get; set; } = Enumerable.Empty<ProductDTO>();

        [BindProperty]
        public PurchaseOrderDTO Order { get; set; } = new PurchaseOrderDTO();

        public async Task OnGet()
        {
            Suppliers = await _supplierService.GetAllSuppliersAsync();
            Products = await _productService.GetAllProductsAsync();
        }

        public async Task<IActionResult> OnPostAddItemAsync(Guid productId, int quantity, decimal unitPrice)
        {
            Suppliers = await _supplierService.GetAllSuppliersAsync();
            Products = await _productService.GetAllProductsAsync();

            if (Order == null)
                Order = new PurchaseOrderDTO();

            if (productId != Guid.Empty && quantity > 0 && unitPrice > 0)
            {
                var product = Products.FirstOrDefault(p => p.ProductID == productId);
                Order.OrderItems.Add(new PurchaseOrderItemDTO
                {
                    PurchaseOrderItemID = Guid.NewGuid(),
                    ProductID = productId,
                    ProductName = product?.ProductName ?? string.Empty,
                    OrderedQuantity = quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = unitPrice * quantity
                });
                Order.TotalAmount = Order.OrderItems.Sum(i => i.TotalPrice);
                Order.DueAmount = Order.TotalAmount - Order.PaidAmount;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            Suppliers = await _supplierService.GetAllSuppliersAsync();
            Products = await _productService.GetAllProductsAsync();

            if (Order == null || Order.SupplierID == Guid.Empty || !Order.OrderItems.Any())
            {
                ModelState.AddModelError(string.Empty, "Supplier and at least one item are required.");
                return Page();
            }

            // In absence of auth, leave CreatedBy empty
            Order.CreatedBy = Order.CreatedBy == Guid.Empty ? Guid.Empty : Order.CreatedBy;
            var created = await _purchaseOrderService.CreatePurchaseOrderAsync(Order);
            TempData["Message"] = $"Purchase Order {created.OrderNumber} created.";
            return RedirectToPage("Index");
        }
    }
}


