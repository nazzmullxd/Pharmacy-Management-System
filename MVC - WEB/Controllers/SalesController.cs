using Microsoft.AspNetCore.Mvc;
using Business.Interfaces;
using Business.DTO;
using MVC_WEB.Filters;
using MVC_WEB.Models.ViewModels;

namespace MVC_WEB.Controllers
{
    [Authenticated]
    public class SalesController : Controller
    {
        private readonly ISalesService _salesService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly ILogger<SalesController> _logger;

        public SalesController(
            ISalesService salesService,
            ICustomerService customerService,
            IProductService productService,
            ILogger<SalesController> logger)
        {
            _salesService = salesService ?? throw new ArgumentNullException(nameof(salesService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: Sales
        public async Task<IActionResult> Index(string? searchTerm, string? statusFilter, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var userRole = HttpContext.Session.GetString("role") ?? "User";
                var userId = GetCurrentUserId();
                
                IEnumerable<SaleDTO> allSales;
                
                // RBAC: Users can only see their own sales, Admins see all
                if (userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    allSales = await _salesService.GetAllSalesAsync();
                }
                else
                {
                    allSales = await _salesService.GetSalesByUserAsync(userId);
                }

                // Apply filters
                var filteredSales = ApplyFilters(allSales, searchTerm, statusFilter, startDate, endDate);

                // Order by date descending and take top 50
                var salesList = filteredSales
                    .OrderByDescending(s => s.SaleDate)
                    .Take(50)
                    .ToList();

                var viewModel = new SalesIndexViewModel
                {
                    Sales = salesList,
                    SearchTerm = searchTerm,
                    StatusFilter = statusFilter,
                    StartDate = startDate,
                    EndDate = endDate
                };

                // Calculate summaries
                CalculateSummaryStatistics(viewModel, allSales);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sales data");
                TempData["Error"] = "Failed to load sales data. Please try again.";
                return View(new SalesIndexViewModel());
            }
        }

        private IEnumerable<SaleDTO> ApplyFilters(IEnumerable<SaleDTO> sales, string? searchTerm, string? statusFilter, DateTime? startDate, DateTime? endDate)
        {
            var filtered = sales;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                filtered = filtered.Where(s =>
                    (s.CustomerName?.ToLower().Contains(term) ?? false) ||
                    s.SaleID.ToString().ToLower().Contains(term) ||
                    (s.UserName?.ToLower().Contains(term) ?? false));
            }

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                filtered = filtered.Where(s => s.PaymentStatus.Equals(statusFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (startDate.HasValue)
            {
                filtered = filtered.Where(s => s.SaleDate >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                filtered = filtered.Where(s => s.SaleDate < endDate.Value.Date.AddDays(1));
            }

            return filtered;
        }

        private void CalculateSummaryStatistics(SalesIndexViewModel viewModel, IEnumerable<SaleDTO> allSales)
        {
            try
            {
                var utcToday = DateTime.UtcNow.Date;
                var utcMonthStart = new DateTime(utcToday.Year, utcToday.Month, 1);

                var todaySlice = allSales.Where(s => s.SaleDate >= utcToday && s.SaleDate < utcToday.AddDays(1)).ToList();
                viewModel.TodaySales = todaySlice.Sum(s => s.TotalAmount);
                viewModel.TodaySalesCount = todaySlice.Count;

                var monthSlice = allSales.Where(s => s.SaleDate >= utcMonthStart && s.SaleDate < utcToday.AddDays(1)).ToList();
                viewModel.MonthlySales = monthSlice.Sum(s => s.TotalAmount);
                viewModel.MonthlySalesCount = monthSlice.Count;

                viewModel.PendingOrders = allSales.Count(s => s.PaymentStatus == SaleDTO.PaymentStatuses.Pending);
                viewModel.TotalOrders = allSales.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating summary statistics");
            }
        }

        // GET: Sales/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var sale = await _salesService.GetSaleByIdAsync(id);
                if (sale == null)
                {
                    TempData["Error"] = "Sale not found.";
                    return RedirectToAction(nameof(Index));
                }

                // RBAC: Users can only view their own sales
                var userRole = HttpContext.Session.GetString("role") ?? "User";
                var userId = GetCurrentUserId();
                
                if (!userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) && sale.UserID != userId)
                {
                    TempData["Error"] = "You don't have permission to view this sale.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new SalesDetailsViewModel
                {
                    Sale = sale,
                    CanMarkAsPaid = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) && sale.PaymentStatus == "Pending",
                    CanEdit = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase),
                    CanDelete = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sale details for {SaleId}", id);
                TempData["Error"] = "Failed to load sale details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Sales/Create
        public async Task<IActionResult> Create(Guid? customerId, Guid? productId)
        {
            try
            {
                var viewModel = await PrepareCreateViewModelAsync();
                
                // Handle pre-selected customer from query string
                if (customerId.HasValue)
                {
                    var customer = await _customerService.GetCustomerByIdAsync(customerId.Value);
                    if (customer != null)
                    {
                        viewModel.SelectedCustomerID = customer.CustomerID;
                        viewModel.Sale.CustomerID = customer.CustomerID;
                        viewModel.Sale.CustomerName = customer.CustomerName;
                    }
                }

                // Handle pre-selected product from query string
                if (productId.HasValue)
                {
                    viewModel.ProductIdToAdd = productId.Value;
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create sale page");
                TempData["Error"] = "Unable to load the create sale page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<SalesCreateViewModel> PrepareCreateViewModelAsync()
        {
            var products = await _productService.GetAllProductsAsync();
            var customers = await _customerService.GetAllCustomersAsync();

            var viewModel = new SalesCreateViewModel
            {
                Products = products.Where(p => p.IsActive).OrderBy(p => p.ProductName),
                Customers = customers.OrderBy(c => c.CustomerName),
                CurrentUser = HttpContext.Session.GetString("userName") ?? "System User"
            };

            // Initialize Sale
            viewModel.Sale.SaleID = Guid.NewGuid();
            viewModel.Sale.SaleDate = DateTime.UtcNow;
            viewModel.Sale.PaymentStatus = "Paid";
            viewModel.Sale.SaleItems = new List<SaleItemDTO>();
            viewModel.Sale.UserID = GetCurrentUserId();

            return viewModel;
        }

        // POST: Sales/AddItem (AJAX handler for adding items to cart)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(SalesCreateViewModel model)
        {
            try
            {
                // Reload reference data
                var viewModel = await PrepareCreateViewModelAsync();
                
                // Preserve existing cart items
                if (model.Sale?.SaleItems != null)
                {
                    viewModel.Sale.SaleItems = model.Sale.SaleItems;
                }
                
                // Preserve customer selection
                viewModel.SelectedCustomerID = model.SelectedCustomerID;
                viewModel.Sale.CustomerID = model.Sale?.CustomerID ?? Guid.Empty;
                viewModel.Sale.CustomerName = model.Sale?.CustomerName;
                viewModel.Sale.PaymentStatus = model.Sale?.PaymentStatus ?? "Paid";
                viewModel.Sale.Note = model.Sale?.Note;

                // Validate product selection
                if (!model.ProductIdToAdd.HasValue || model.ProductIdToAdd == Guid.Empty)
                {
                    TempData["Error"] = "Please select a product.";
                    RecomputeTotals(viewModel);
                    return View("Create", viewModel);
                }

                if (model.QuantityToAdd <= 0)
                {
                    TempData["Error"] = "Quantity must be greater than zero.";
                    RecomputeTotals(viewModel);
                    return View("Create", viewModel);
                }

                var product = viewModel.Products.FirstOrDefault(p => p.ProductID == model.ProductIdToAdd.Value);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    RecomputeTotals(viewModel);
                    return View("Create", viewModel);
                }

                // Check stock availability
                var existingItem = viewModel.Sale.SaleItems?.FirstOrDefault(i => i.ProductID == product.ProductID);
                var totalRequestedQuantity = (existingItem?.Quantity ?? 0) + model.QuantityToAdd;
                
                if (product.TotalStock < totalRequestedQuantity)
                {
                    TempData["Error"] = $"Insufficient stock. Available: {product.TotalStock}, Requested: {totalRequestedQuantity}";
                    RecomputeTotals(viewModel);
                    return View("Create", viewModel);
                }

                // Add or update item
                if (existingItem != null)
                {
                    existingItem.Quantity += model.QuantityToAdd;
                    existingItem.RecomputeTotal();
                }
                else
                {
                    var newItem = new SaleItemDTO
                    {
                        SaleItemID = Guid.NewGuid(),
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,
                        Quantity = model.QuantityToAdd,
                        UnitPrice = product.UnitPrice,
                        Discount = 0m,
                        BatchNumber = "N/A"
                    };
                    newItem.RecomputeTotal();
                    viewModel.Sale.SaleItems!.Add(newItem);
                }

                RecomputeTotals(viewModel);
                TempData["Success"] = $"Added {model.QuantityToAdd} x {product.ProductName} to cart.";
                
                return View("Create", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to sale");
                TempData["Error"] = "Error adding item. Please try again.";
                return RedirectToAction(nameof(Create));
            }
        }

        // POST: Sales/RemoveItem (handler for removing items from cart)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(SalesCreateViewModel model, int index)
        {
            try
            {
                var viewModel = await PrepareCreateViewModelAsync();
                
                // Preserve existing cart items
                if (model.Sale?.SaleItems != null)
                {
                    viewModel.Sale.SaleItems = model.Sale.SaleItems;
                }
                
                // Preserve customer selection
                viewModel.SelectedCustomerID = model.SelectedCustomerID;
                viewModel.Sale.CustomerID = model.Sale?.CustomerID ?? Guid.Empty;
                viewModel.Sale.CustomerName = model.Sale?.CustomerName;
                viewModel.Sale.PaymentStatus = model.Sale?.PaymentStatus ?? "Paid";
                viewModel.Sale.Note = model.Sale?.Note;

                if (viewModel.Sale.SaleItems != null && index >= 0 && index < viewModel.Sale.SaleItems.Count)
                {
                    var removedItem = viewModel.Sale.SaleItems[index];
                    viewModel.Sale.SaleItems.RemoveAt(index);
                    TempData["Success"] = $"Removed {removedItem.ProductName} from cart.";
                }
                else
                {
                    TempData["Error"] = "Invalid item to remove.";
                }

                RecomputeTotals(viewModel);
                ModelState.Clear();
                
                return View("Create", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from sale");
                TempData["Error"] = "Error removing item. Please try again.";
                return RedirectToAction(nameof(Create));
            }
        }

        // POST: Sales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SalesCreateViewModel model, string? handler)
        {
            try
            {
                var viewModel = await PrepareCreateViewModelAsync();
                
                // Preserve cart items
                if (model.Sale?.SaleItems != null && model.Sale.SaleItems.Any())
                {
                    viewModel.Sale.SaleItems = model.Sale.SaleItems;
                }
                else
                {
                    TempData["Error"] = "At least one item is required for the sale.";
                    return View(viewModel);
                }

                // Validate and process customer
                await ProcessCustomerAsync(viewModel, model);

                // Set sale properties
                viewModel.Sale.SaleID = Guid.NewGuid();
                viewModel.Sale.SaleDate = DateTime.UtcNow;
                viewModel.Sale.PaymentStatus = model.Sale?.PaymentStatus ?? "Paid";
                viewModel.Sale.Note = model.Sale?.Note;
                viewModel.Sale.UserID = GetCurrentUserId();
                viewModel.Sale.UserName = HttpContext.Session.GetString("userName") ?? "System User";

                // Prepare items
                foreach (var item in viewModel.Sale.SaleItems)
                {
                    if (item.SaleItemID == Guid.Empty)
                    {
                        item.SaleItemID = Guid.NewGuid();
                    }
                    item.SaleID = viewModel.Sale.SaleID;
                    item.BatchNumber = string.IsNullOrWhiteSpace(item.BatchNumber) ? "N/A" : item.BatchNumber;
                    item.RecomputeTotal();
                }

                RecomputeTotals(viewModel);

                // Create the sale
                _logger.LogInformation("Creating sale with {ItemCount} items, Total: {Total:C}", 
                    viewModel.Sale.SaleItems.Count, viewModel.Sale.TotalAmount);

                await _salesService.CreateSaleAsync(viewModel.Sale);

                _logger.LogInformation("Successfully created sale {SaleId} for customer {CustomerName} with total {Total:C}", 
                    viewModel.Sale.SaleID, viewModel.Sale.CustomerName, viewModel.Sale.TotalAmount);

                TempData["Success"] = $"Sale created successfully! Sale ID: #{viewModel.Sale.SaleID.ToString().Substring(0, 8)} - Total: {viewModel.Sale.TotalAmount:C}";
                TempData["SaleID"] = viewModel.Sale.SaleID.ToString();

                // Handle different submit actions
                if (handler == "SaveAndPrint")
                {
                    return RedirectToAction(nameof(Invoice), new { id = viewModel.Sale.SaleID });
                }
                else if (handler == "SaveAndNew")
                {
                    return RedirectToAction(nameof(Create));
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sale");
                TempData["Error"] = "Error creating sale. Please try again.";
                
                var newViewModel = await PrepareCreateViewModelAsync();
                if (model.Sale?.SaleItems != null)
                {
                    newViewModel.Sale.SaleItems = model.Sale.SaleItems;
                }
                return View(newViewModel);
            }
        }

        private async Task ProcessCustomerAsync(SalesCreateViewModel viewModel, SalesCreateViewModel model)
        {
            if (model.SelectedCustomerID.HasValue && model.SelectedCustomerID.Value != Guid.Empty)
            {
                var existingCustomer = await _customerService.GetCustomerByIdAsync(model.SelectedCustomerID.Value);
                if (existingCustomer != null)
                {
                    viewModel.Sale.CustomerID = existingCustomer.CustomerID;
                    viewModel.Sale.CustomerName = existingCustomer.CustomerName;
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(model.Sale?.CustomerName))
            {
                var matches = await _customerService.SearchCustomersByNameAsync(model.Sale.CustomerName);
                var exactMatch = matches.FirstOrDefault(c =>
                    c.CustomerName.Equals(model.Sale.CustomerName, StringComparison.OrdinalIgnoreCase));

                if (exactMatch != null)
                {
                    viewModel.Sale.CustomerID = exactMatch.CustomerID;
                    viewModel.Sale.CustomerName = exactMatch.CustomerName;
                    return;
                }

                // Create new customer
                var newCustomer = new CustomerDTO
                {
                    CustomerName = model.Sale.CustomerName.Trim(),
                    ContactNumber = "N/A",
                    Email = string.Empty,
                    Address = string.Empty,
                    CreatedDate = DateTime.UtcNow
                };

                var createdCustomer = await _customerService.CreateCustomerAsync(newCustomer);
                viewModel.Sale.CustomerID = createdCustomer.CustomerID;
                viewModel.Sale.CustomerName = createdCustomer.CustomerName;
                return;
            }

            // Walk-in customer
            viewModel.Sale.CustomerName = "Walk-in Customer";
            viewModel.Sale.CustomerID = new Guid("00000000-0000-0000-0000-000000000001");
        }

        private void RecomputeTotals(SalesCreateViewModel viewModel)
        {
            if (viewModel.Sale?.SaleItems == null) return;

            foreach (var item in viewModel.Sale.SaleItems)
            {
                item.BatchNumber = string.IsNullOrWhiteSpace(item.BatchNumber) ? "N/A" : item.BatchNumber;
                item.RecomputeTotal();
                if (item.TotalPrice < 0) item.TotalPrice = 0;
            }

            viewModel.Sale.TotalAmount = viewModel.Sale.SaleItems.Sum(i => i.TotalPrice);
        }

        // GET: Sales/Invoice/5
        public async Task<IActionResult> Invoice(Guid id)
        {
            try
            {
                var sale = await _salesService.GetSaleByIdAsync(id);
                if (sale == null)
                {
                    TempData["Error"] = $"Invoice not found for Sale ID: {id}";
                    return RedirectToAction(nameof(Index));
                }

                // RBAC: Users can only view their own invoices
                var userRole = HttpContext.Session.GetString("role") ?? "User";
                var userId = GetCurrentUserId();
                
                if (!userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) && sale.UserID != userId)
                {
                    TempData["Error"] = "You don't have permission to view this invoice.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new SalesInvoiceViewModel
                {
                    Sale = sale
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading invoice for sale {SaleId}", id);
                TempData["Error"] = "Error loading invoice. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Sales/MarkAsPaid
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> MarkAsPaid(Guid saleId, string? returnUrl)
        {
            if (saleId == Guid.Empty)
            {
                TempData["Error"] = "Invalid sale identifier.";
                return RedirectToReturnUrl(returnUrl);
            }

            try
            {
                var ok = await _salesService.UpdatePaymentStatusAsync(saleId, SaleDTO.PaymentStatuses.Paid);
                TempData[ok ? "Success" : "Error"] =
                    ok ? "Sale marked as paid successfully." : "Failed to update payment status.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for sale {SaleId}", saleId);
                TempData["Error"] = "Error updating payment status. Please try again.";
            }

            return RedirectToReturnUrl(returnUrl);
        }

        // API: Get product stock for AJAX validation
        [HttpGet]
        public async Task<IActionResult> GetProductStock(Guid productId)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return Json(new ProductStockResult { Success = false, Message = "Product not found" });
                }

                return Json(new ProductStockResult
                {
                    Success = true,
                    Stock = product.TotalStock,
                    Price = product.UnitPrice,
                    Name = product.ProductName,
                    IsActive = product.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product stock for {ProductId}", productId);
                return Json(new ProductStockResult { Success = false, Message = "Error retrieving product information" });
            }
        }

        // API: Search customers for autocomplete
        [HttpGet]
        public async Task<IActionResult> SearchCustomers(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                {
                    return Json(new CustomerSearchResult { Success = true, Customers = new List<CustomerSearchItem>() });
                }

                var customers = await _customerService.SearchCustomersByNameAsync(term);
                var result = customers.Take(10).Select(c => new CustomerSearchItem
                {
                    Id = c.CustomerID,
                    Name = c.CustomerName,
                    Contact = c.ContactNumber,
                    Email = c.Email
                });

                return Json(new CustomerSearchResult { Success = true, Customers = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers with term {Term}", term);
                return Json(new CustomerSearchResult { Success = false, Customers = new List<CustomerSearchItem>() });
            }
        }

        private Guid GetCurrentUserId()
        {
            var sessionUser = HttpContext.Session.GetString("userId");
            if (!string.IsNullOrWhiteSpace(sessionUser) && Guid.TryParse(sessionUser, out var sessionId))
            {
                return sessionId;
            }

            // Fallback
            _logger.LogWarning("Using fallback user ID");
            return new Guid("6F50D18A-B772-40B0-B321-14464010A9A8");
        }

        private IActionResult RedirectToReturnUrl(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
