using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MVC_WEB.Filters;
using MVC_WEB.Models.ViewModels;

namespace MVC_WEB.Controllers
{
    [Authenticated]
    [AdminOnly] // Purchase Management is Admin-only
    public class PurchasesController : Controller
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;

        public PurchasesController(
            IPurchaseOrderService purchaseOrderService,
            ISupplierService supplierService,
            IProductService productService)
        {
            _purchaseOrderService = purchaseOrderService;
            _supplierService = supplierService;
            _productService = productService;
        }

        // GET: Purchases
        public async Task<IActionResult> Index()
        {
            var orders = await _purchaseOrderService.GetAllPurchaseOrdersAsync() ?? Enumerable.Empty<PurchaseOrderDTO>();
            
            var viewModel = new PurchasesIndexViewModel
            {
                Orders = orders,
                TotalOrders = orders.Count(),
                TotalValue = orders.Sum(o => o.TotalAmount),
                PendingOrders = orders.Count(o => o.Status == "Pending"),
                ProcessedOrders = orders.Count(o => o.Status == "Processed"),
                OutstandingAmount = orders.Sum(o => o.DueAmount)
            };
            
            return View(viewModel);
        }

        // GET: Purchases/Orders (redirect to Index)
        public IActionResult Orders()
        {
            return RedirectToAction(nameof(Index));
        }

        // GET: Purchases/New (redirect to Create)
        public IActionResult New()
        {
            return RedirectToAction(nameof(Create));
        }

        // GET: Purchases/Create
        public async Task<IActionResult> Create()
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            var products = await _productService.GetAllProductsAsync();
            
            var viewModel = new PurchasesCreateViewModel
            {
                Suppliers = suppliers,
                Products = products,
                Order = new PurchaseOrderDTO
                {
                    PurchaseOrderID = Guid.NewGuid(),
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    PaymentStatus = "Pending",
                    PaidAmount = 0,
                    TotalAmount = 0,
                    DueAmount = 0,
                    CreatedBy = GetCurrentUserId(),
                    OrderItems = new List<PurchaseOrderItemDTO>()
                }
            };
            
            return View(viewModel);
        }

        // POST: Purchases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchasesCreateViewModel viewModel)
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            var products = await _productService.GetAllProductsAsync();
            viewModel.Suppliers = suppliers;
            viewModel.Products = products;

            if (viewModel.Order == null)
            {
                ModelState.AddModelError(string.Empty, "Order data is missing. Please try again.");
                return View(viewModel);
            }

            // Basic validation
            var isValid = true;

            if (viewModel.Order.SupplierID == Guid.Empty)
            {
                ModelState.AddModelError("Order.SupplierID", "Please select a supplier.");
                isValid = false;
            }

            if (viewModel.Order.OrderItems == null || !viewModel.Order.OrderItems.Any())
            {
                ModelState.AddModelError(string.Empty, "Please add at least one item to the order.");
                isValid = false;
            }
            else
            {
                for (int i = 0; i < viewModel.Order.OrderItems.Count; i++)
                {
                    var item = viewModel.Order.OrderItems[i];
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
                return View(viewModel);
            }

            try
            {
                // Set required fields
                if (viewModel.Order.PurchaseOrderID == Guid.Empty)
                    viewModel.Order.PurchaseOrderID = Guid.NewGuid();
                
                if (viewModel.Order.CreatedBy == Guid.Empty)
                    viewModel.Order.CreatedBy = GetCurrentUserId();
                
                if (viewModel.Order.OrderDate == default)
                    viewModel.Order.OrderDate = DateTime.Now;

                viewModel.Order.Status = "Pending";
                viewModel.Order.PaymentStatus = "Pending";

                // Recalculate totals
                RecalculateOrderTotals(viewModel.Order);

                var created = await _purchaseOrderService.CreatePurchaseOrderAsync(viewModel.Order);
                if (created != null)
                {
                    TempData["Message"] = $"Purchase Order {created.OrderNumber} created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create purchase order. Please try again.");
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating purchase order: {ex.Message}");
                TempData["Error"] = $"Failed to create purchase order: {ex.Message}";
                return View(viewModel);
            }
        }

        // POST: Purchases/AddItem (AJAX or form post)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(PurchasesCreateViewModel viewModel, Guid productId, int quantity, decimal unitPrice)
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            var products = await _productService.GetAllProductsAsync();
            viewModel.Suppliers = suppliers;
            viewModel.Products = products;

            if (viewModel.Order == null)
            {
                viewModel.Order = new PurchaseOrderDTO
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
                var product = products.FirstOrDefault(p => p.ProductID == productId);
                var totalPrice = unitPrice * quantity;
                
                viewModel.Order.OrderItems.Add(new PurchaseOrderItemDTO
                {
                    PurchaseOrderItemID = Guid.NewGuid(),
                    ProductID = productId,
                    ProductName = product?.ProductName ?? string.Empty,
                    OrderedQuantity = quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                });
                
                RecalculateOrderTotals(viewModel.Order);
            }

            return View("Create", viewModel);
        }

        // POST: Purchases/RemoveItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(PurchasesCreateViewModel viewModel, int removeIndex)
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            var products = await _productService.GetAllProductsAsync();
            viewModel.Suppliers = suppliers;
            viewModel.Products = products;

            if (viewModel.Order?.OrderItems != null && removeIndex >= 0 && removeIndex < viewModel.Order.OrderItems.Count)
            {
                viewModel.Order.OrderItems.RemoveAt(removeIndex);
                RecalculateOrderTotals(viewModel.Order);
            }

            return View("Create", viewModel);
        }

        // GET: Purchases/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var order = await _purchaseOrderService.GetPurchaseOrderByIdAsync(id);
            if (order == null)
            {
                TempData["Error"] = "Purchase order not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new PurchasesDetailsViewModel
            {
                Order = order
            };
            
            return View(viewModel);
        }

        // GET: Purchases/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var order = await _purchaseOrderService.GetPurchaseOrderByIdAsync(id);
            if (order == null)
            {
                TempData["Error"] = "Purchase order not found.";
                return RedirectToAction(nameof(Index));
            }

            var suppliers = await _supplierService.GetAllSuppliersAsync();
            var products = await _productService.GetAllProductsAsync();

            var viewModel = new PurchasesEditViewModel
            {
                Order = order,
                Suppliers = suppliers,
                Products = products
            };
            
            return View(viewModel);
        }

        // POST: Purchases/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PurchasesEditViewModel viewModel)
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            var products = await _productService.GetAllProductsAsync();
            viewModel.Suppliers = suppliers;
            viewModel.Products = products;

            if (viewModel.Order == null)
            {
                ModelState.AddModelError(string.Empty, "Order data is missing.");
                return View(viewModel);
            }

            try
            {
                RecalculateOrderTotals(viewModel.Order);
                var updated = await _purchaseOrderService.UpdatePurchaseOrderAsync(viewModel.Order);
                if (updated != null)
                {
                    TempData["Message"] = $"Purchase Order {updated.OrderNumber} updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to update purchase order.");
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating purchase order: {ex.Message}");
                return View(viewModel);
            }
        }

        // POST: Purchases/ProcessOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrder(Guid orderId)
        {
            try
            {
                var result = await _purchaseOrderService.ProcessPurchaseOrderAsync(orderId, GetCurrentUserId());
                
                if (result)
                {
                    TempData["Message"] = "Purchase order has been processed successfully and inventory has been updated.";
                }
                else
                {
                    TempData["Error"] = "Failed to process the purchase order. Please ensure it's in pending status.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while processing the order: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Purchases/ApproveOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveOrder(Guid orderId)
        {
            try
            {
                var result = await _purchaseOrderService.ApprovePurchaseOrderAsync(orderId, GetCurrentUserId());
                
                if (result)
                {
                    TempData["Message"] = "Purchase order has been approved successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to approve the purchase order.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while approving the order: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Purchases/CancelOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(Guid orderId, string reason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    TempData["Error"] = "Please provide a reason for cancellation.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _purchaseOrderService.CancelPurchaseOrderAsync(orderId, reason);
                
                if (result)
                {
                    TempData["Message"] = "Purchase order has been cancelled successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to cancel the purchase order.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while cancelling the order: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Purchases/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _purchaseOrderService.DeletePurchaseOrderAsync(id);
                if (result)
                {
                    TempData["Message"] = "Purchase order deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to delete the purchase order.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting purchase order: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        #region Helper Methods
        
        private Guid GetCurrentUserId()
        {
            var userIdString = HttpContext.Session.GetString("userId");
            if (Guid.TryParse(userIdString, out var userId))
            {
                return userId;
            }
            return Guid.Empty;
        }

        private void RecalculateOrderTotals(PurchaseOrderDTO order)
        {
            if (order?.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {
                    item.TotalPrice = item.OrderedQuantity * item.UnitPrice;
                }
                
                order.TotalAmount = order.OrderItems.Sum(i => i.TotalPrice);
                order.DueAmount = order.TotalAmount - order.PaidAmount;
            }
        }

        #endregion
    }
}
