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
            
            // Initialize Order with default values
            Order = new PurchaseOrderDTO
            {
                PurchaseOrderID = Guid.NewGuid(),
                OrderDate = DateTime.Now,
                Status = "Pending",
                PaymentStatus = "Pending",
                PaidAmount = 0,
                TotalAmount = 0,
                DueAmount = 0,
                CreatedBy = GetCurrentUserId(), // Use a proper method to get user ID
                OrderItems = new List<PurchaseOrderItemDTO>()
            };
        }

        public async Task<IActionResult> OnPostAddItemAsync(Guid productId, int quantity, decimal unitPrice)
        {
            Suppliers = await _supplierService.GetAllSuppliersAsync();
            Products = await _productService.GetAllProductsAsync();

            if (Order == null)
            {
                Order = new PurchaseOrderDTO
                {
                    PurchaseOrderID = Guid.NewGuid(),
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    PaymentStatus = "Pending",
                    PaidAmount = 0,
                    CreatedBy = GetCurrentUserId(),
                    OrderItems = new List<PurchaseOrderItemDTO>()
                };
            }

            if (productId != Guid.Empty && quantity > 0 && unitPrice > 0)
            {
                var product = Products.FirstOrDefault(p => p.ProductID == productId);
                var totalPrice = unitPrice * quantity;
                
                Order.OrderItems.Add(new PurchaseOrderItemDTO
                {
                    PurchaseOrderItemID = Guid.NewGuid(),
                    ProductID = productId,
                    ProductName = product?.ProductName ?? string.Empty,
                    OrderedQuantity = quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                });
                
                // Recalculate totals using consistent method
                RecalculateOrderTotals();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            try
            {
                // Reload data for dropdowns in case we need to redisplay the form
                Suppliers = await _supplierService.GetAllSuppliersAsync();
                Products = await _productService.GetAllProductsAsync();

                // Ensure Order is properly initialized
                if (Order == null)
                {
                    ModelState.AddModelError(string.Empty, "Order data is missing. Please try again.");
                    return Page();
                }

                // Basic validation
                var isValid = true;

                if (Order.SupplierID == Guid.Empty)
                {
                    ModelState.AddModelError("Order.SupplierID", "Please select a supplier.");
                    isValid = false;
                }

                if (Order.OrderItems == null || !Order.OrderItems.Any())
                {
                    ModelState.AddModelError(string.Empty, "Please add at least one item to the order.");
                    isValid = false;
                }
                else
                {
                    // Validate each order item
                    for (int i = 0; i < Order.OrderItems.Count; i++)
                    {
                        var item = Order.OrderItems[i];
                        if (item.ProductID == Guid.Empty)
                        {
                            ModelState.AddModelError($"Order.OrderItems[{i}].ProductID", "Product is required.");
                            isValid = false;
                        }
                        if (item.OrderedQuantity <= 0)
                        {
                            ModelState.AddModelError($"Order.OrderItems[{i}].OrderedQuantity", "Quantity must be greater than 0.");
                            isValid = false;
                        }
                        if (item.UnitPrice <= 0)
                        {
                            ModelState.AddModelError($"Order.OrderItems[{i}].UnitPrice", "Unit price must be greater than 0.");
                            isValid = false;
                        }
                    }
                }

                if (!isValid)
                {
                    return Page();
                }

                // Set required fields if not already set
                if (Order.PurchaseOrderID == Guid.Empty)
                    Order.PurchaseOrderID = Guid.NewGuid();
                
                if (Order.CreatedBy == Guid.Empty)
                    Order.CreatedBy = GetCurrentUserId();
                
                if (Order.OrderDate == default)
                    Order.OrderDate = DateTime.Now;

                // Set default status values
                Order.Status = "Pending";
                Order.PaymentStatus = "Pending";

                // Always recalculate totals before saving
                RecalculateOrderTotals();

                // Create the purchase order
                var created = await _purchaseOrderService.CreatePurchaseOrderAsync(Order);
                if (created != null)
                {
                    TempData["Message"] = $"Purchase Order {created.OrderNumber} created successfully!";
                    return RedirectToPage("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create purchase order. Please try again.");
                    // Reload page data
                    Suppliers = await _supplierService.GetAllSuppliersAsync();
                    Products = await _productService.GetAllProductsAsync();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating purchase order: {ex.Message}");
                TempData["Error"] = $"Failed to create purchase order: {ex.Message}";
                // Reload page data
                Suppliers = await _supplierService.GetAllSuppliersAsync();
                Products = await _productService.GetAllProductsAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostRemoveItemAsync(int removeIndex)
        {
            // Reload the data
            Suppliers = await _supplierService.GetAllSuppliersAsync();
            Products = await _productService.GetAllProductsAsync();

            // Remove the item at the specified index
            if (Order?.OrderItems != null && removeIndex >= 0 && removeIndex < Order.OrderItems.Count)
            {
                Order.OrderItems.RemoveAt(removeIndex);
                
                // Recalculate totals using consistent method
                RecalculateOrderTotals();
            }

            return Page();
        }

        private Guid GetCurrentUserId()
        {
            // TODO: Replace with actual authentication logic
            // For now, return a consistent default user ID
            // In a real application, you would get this from HttpContext.User or similar
            return Guid.Parse("11111111-1111-1111-1111-111111111111");
        }

        private void RecalculateOrderTotals()
        {
            if (Order?.OrderItems != null)
            {
                // Ensure each item has correct TotalPrice
                foreach (var item in Order.OrderItems)
                {
                    item.TotalPrice = item.OrderedQuantity * item.UnitPrice;
                }
                
                // Calculate order totals
                Order.TotalAmount = Order.OrderItems.Sum(i => i.TotalPrice);
                Order.DueAmount = Order.TotalAmount - Order.PaidAmount;
            }
        }
    }
}



