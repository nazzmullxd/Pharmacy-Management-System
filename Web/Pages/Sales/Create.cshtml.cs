using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace Web.Pages.Sales
{
    public class CreateModel : PageModel
    {
        private readonly ISalesService _salesService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            ISalesService salesService,
            ICustomerService customerService,
            IProductService productService,
            ILogger<CreateModel> logger)
        {
            _salesService = salesService ?? throw new ArgumentNullException(nameof(salesService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IEnumerable<ProductDTO> Products { get; private set; } = Enumerable.Empty<ProductDTO>();
        public IEnumerable<CustomerDTO> Customers { get; private set; } = Enumerable.Empty<CustomerDTO>();

        [BindProperty]
        public SaleDTO Sale { get; set; } = new();

        [BindProperty]
        public Guid? SelectedCustomerID { get; set; }

        [BindProperty]
        public Guid? ProductIdToAdd { get; set; }

        [BindProperty]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int QuantityToAdd { get; set; } = 1;

        // Additional properties for enhanced functionality
        public string? CurrentUser { get; private set; }
        public bool HasValidationErrors => !ModelState.IsValid;
        public int TotalItems => Sale?.SaleItems?.Count ?? 0;

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadReferenceDataAsync();
                InitializeSale();

                // Handle pre-selected customer from query string
                if (Request.Query.TryGetValue("customerId", out var customerIdValue) &&
                    Guid.TryParse(customerIdValue, out var customerId))
                {
                    await PreSelectCustomerAsync(customerId);
                }

                // Handle pre-selected product from query string
                if (Request.Query.TryGetValue("productId", out var productIdValue) &&
                    Guid.TryParse(productIdValue, out var productId))
                {
                    ProductIdToAdd = productId;
                }

                CurrentUser = GetCurrentUserName();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create sale page");
                TempData["ErrorMessage"] = "Unable to load the create sale page. Please try again.";
                return RedirectToPage("Index");
            }
        }

        private async Task LoadReferenceDataAsync()
        {
            try
            {
                // Load data sequentially to avoid DbContext concurrency issues
                var products = await _productService.GetAllProductsAsync();
                var customers = await _customerService.GetAllCustomersAsync();

                Products = products.Where(p => p.IsActive).OrderBy(p => p.ProductName);
                Customers = customers.OrderBy(c => c.CustomerName);

                _logger.LogInformation("Loaded {ProductCount} products and {CustomerCount} customers", 
                    Products.Count(), Customers.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reference data");
                throw new InvalidOperationException("Failed to load product and customer data", ex);
            }
        }

        private void InitializeSale()
        {
            Sale.SaleID = Guid.NewGuid();
            Sale.SaleDate = DateTime.UtcNow;
            Sale.PaymentStatus = "Paid"; // Default to paid
            Sale.SaleItems ??= new List<SaleItemDTO>();
            Sale.UserID = ResolveUserId();
        }

        private async Task PreSelectCustomerAsync(Guid customerId)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(customerId);
                if (customer != null)
                {
                    SelectedCustomerID = customer.CustomerID;
                    Sale.CustomerID = customer.CustomerID;
                    Sale.CustomerName = customer.CustomerName;
                    _logger.LogInformation("Pre-selected customer: {CustomerName}", customer.CustomerName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to pre-select customer {CustomerId}", customerId);
            }
        }

        private string GetCurrentUserName()
        {
            try
            {
                return User.Identity?.Name ?? "System User";
            }
            catch
            {
                return "Unknown User";
            }
        }

        private void RecomputeTotals()
        {
            if (Sale?.SaleItems == null) return;

            try
            {
                foreach (var item in Sale.SaleItems)
                {
                    // Ensure batch number is not null
                    item.BatchNumber = string.IsNullOrWhiteSpace(item.BatchNumber) ? "N/A" : item.BatchNumber;
                    
                    // Recompute item total
                    item.RecomputeTotal();
                    
                    // Validate totals are not negative
                    if (item.TotalPrice < 0)
                    {
                        _logger.LogWarning("Negative total price calculated for item {ProductName}: {Total}", 
                            item.ProductName, item.TotalPrice);
                        item.TotalPrice = 0;
                    }
                }

                // Calculate grand total
                Sale.TotalAmount = Sale.SaleItems.Sum(i => i.TotalPrice);
                
                _logger.LogDebug("Recomputed totals: {ItemCount} items, Total: {Total:C}", 
                    Sale.SaleItems.Count, Sale.TotalAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recomputing totals");
                Sale.TotalAmount = 0;
            }
        }

        private Guid ResolveUserId()
        {
            try
            {
                // Try to get user ID from claims
                var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null && Guid.TryParse(claim.Value, out var claimId))
                {
                    return claimId;
                }

                // Try to get user ID from session
                var sessionUser = HttpContext.Session.GetString("UserId");
                if (!string.IsNullOrWhiteSpace(sessionUser) && Guid.TryParse(sessionUser, out var sessionId))
                {
                    return sessionId;
                }

                // Fallback for testing - use a default user (first user from database)
                // TODO: Implement proper authentication system
                _logger.LogWarning("Using fallback user ID for testing - implement proper authentication");
                return new Guid("6F50D18A-B772-40B0-B321-14464010A9A8"); // Nazmul user from database
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving user ID");
                return new Guid("6F50D18A-B772-40B0-B321-14464010A9A8"); // Fallback to first user
            }
        }

        public async Task<IActionResult> OnPostAddItemAsync()
        {
            try
            {
                await LoadReferenceDataAsync();
                Sale.SaleItems ??= new List<SaleItemDTO>();

                // Validate input
                if (!ValidateAddItemInput())
                {
                    RecomputeTotals();
                    return Page();
                }

                var product = await GetProductByIdAsync(ProductIdToAdd!.Value);
                if (product == null)
                {
                    ModelState.AddModelError(nameof(ProductIdToAdd), "Product not found.");
                    RecomputeTotals();
                    return Page();
                }

                // Check stock availability
                if (!await ValidateStockAvailabilityAsync(product, QuantityToAdd))
                {
                    RecomputeTotals();
                    return Page();
                }

                // Add or update item
                await AddOrUpdateSaleItemAsync(product, QuantityToAdd);

                RecomputeTotals();
                ResetAddItemForm();

                TempData["SuccessMessage"] = $"Added {QuantityToAdd} x {product.ProductName} to cart.";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to sale");
                ModelState.AddModelError(string.Empty, "Error adding item. Please try again.");
                return Page();
            }
        }

        private bool ValidateAddItemInput()
        {
            if (ProductIdToAdd is null || ProductIdToAdd == Guid.Empty)
            {
                ModelState.AddModelError(nameof(ProductIdToAdd), "Please select a product.");
                return false;
            }

            if (QuantityToAdd <= 0)
            {
                ModelState.AddModelError(nameof(QuantityToAdd), "Quantity must be greater than zero.");
                return false;
            }

            return true;
        }

        private Task<ProductDTO?> GetProductByIdAsync(Guid productId)
        {
            try
            {
                var product = Products.FirstOrDefault(p => p.ProductID == productId);
                return Task.FromResult(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product {ProductId}", productId);
                return Task.FromResult<ProductDTO?>(null);
            }
        }

        private Task<bool> ValidateStockAvailabilityAsync(ProductDTO product, int requestedQuantity)
        {
            try
            {
                // Check if product has sufficient stock
                if (product.TotalStock < requestedQuantity)
                {
                    ModelState.AddModelError(nameof(QuantityToAdd), 
                        $"Insufficient stock. Available: {product.TotalStock}, Requested: {requestedQuantity}");
                    return Task.FromResult(false);
                }

                // Check if adding to existing item would exceed stock
                var existingItem = Sale.SaleItems?.FirstOrDefault(i => i.ProductID == product.ProductID);
                if (existingItem != null)
                {
                    var totalRequestedQuantity = existingItem.Quantity + requestedQuantity;
                    if (product.TotalStock < totalRequestedQuantity)
                    {
                        ModelState.AddModelError(nameof(QuantityToAdd), 
                            $"Total quantity would exceed stock. Available: {product.TotalStock}, Total requested: {totalRequestedQuantity}");
                        return Task.FromResult(false);
                    }
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating stock availability for product {ProductId}", product.ProductID);
                ModelState.AddModelError(string.Empty, "Error checking stock availability.");
                return Task.FromResult(false);
            }
        }

        private Task AddOrUpdateSaleItemAsync(ProductDTO product, int quantity)
        {
            try
            {
                // Check if product already exists in sale items
                var existingItem = Sale.SaleItems?.FirstOrDefault(i => i.ProductID == product.ProductID);
                
                if (existingItem != null)
                {
                    // Update existing item
                    existingItem.Quantity += quantity;
                    existingItem.RecomputeTotal();
                    _logger.LogInformation("Updated existing item: {ProductName}, New quantity: {Quantity}", 
                        product.ProductName, existingItem.Quantity);
                }
                else
                {
                    // Add new item
                    var newItem = new SaleItemDTO
                    {
                        SaleItemID = Guid.NewGuid(),
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,
                        Quantity = quantity,
                        UnitPrice = product.UnitPrice,
                        Discount = 0m,
                        BatchNumber = "N/A"
                    };
                    newItem.RecomputeTotal();
                    
                    Sale.SaleItems ??= new List<SaleItemDTO>();
                    Sale.SaleItems.Add(newItem);
                    
                    _logger.LogInformation("Added new item: {ProductName}, Quantity: {Quantity}", 
                        product.ProductName, quantity);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding/updating sale item");
                throw;
            }
        }

        private void ResetAddItemForm()
        {
            ProductIdToAdd = null;
            QuantityToAdd = 1;
            
            // Clear model state for these fields to prevent validation errors on page render
            ModelState.Remove(nameof(ProductIdToAdd));
            ModelState.Remove(nameof(QuantityToAdd));
            
            // Also clear any validation messages for the product selection
            var productKeys = ModelState.Keys.Where(k => k.Contains(nameof(ProductIdToAdd))).ToList();
            foreach (var key in productKeys)
            {
                ModelState.Remove(key);
            }
        }

        public async Task<IActionResult> OnPostRemoveItemAsync(int index)
        {
            try
            {
                await LoadReferenceDataAsync();
                
                if (Sale?.SaleItems != null && index >= 0 && index < Sale.SaleItems.Count)
                {
                    var removedItem = Sale.SaleItems[index];
                    Sale.SaleItems.RemoveAt(index);
                    
                    _logger.LogInformation("Removed item: {ProductName} from sale", removedItem.ProductName);
                    TempData["SuccessMessage"] = $"Removed {removedItem.ProductName} from cart.";
                }
                else
                {
                    _logger.LogWarning("Attempted to remove item at invalid index: {Index}", index);
                    TempData["ErrorMessage"] = "Invalid item to remove.";
                }

                RecomputeTotals();
                ModelState.Clear(); // Clear all model state to prevent validation issues
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from sale");
                TempData["ErrorMessage"] = "Error removing item. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await LoadReferenceDataAsync();
                Sale.SaleItems ??= new List<SaleItemDTO>();

                // Clear add-item related model state
                ModelState.Remove(nameof(ProductIdToAdd));
                ModelState.Remove(nameof(QuantityToAdd));
                
                // Remove CustomerName validation errors for walk-in customers
                ModelState.Remove($"{nameof(Sale)}.{nameof(Sale.CustomerName)}");
                // Also remove any nested validation for CustomerName
                var customerNameKeys = ModelState.Keys.Where(k => k.Contains("CustomerName")).ToList();
                foreach (var key in customerNameKeys)
                {
                    ModelState.Remove(key);
                }

                RecomputeTotals();

                // Comprehensive validation
                if (!await ValidateSaleAsync())
                {
                    return Page();
                }

                // Prepare sale for creation
                await PrepareSaleForCreationAsync();

                // Create the sale
                _logger.LogInformation("Creating sale with {ItemCount} items, Total: {Total:C}", 
                    Sale.SaleItems.Count, Sale.TotalAmount);
                
                foreach (var item in Sale.SaleItems)
                {
                    _logger.LogInformation("Sale item: {ProductName}, Quantity: {Quantity}, Price: {Price:C}", 
                        item.ProductName, item.Quantity, item.UnitPrice);
                }
                
                await _salesService.CreateSaleAsync(Sale);
                
                _logger.LogInformation("Successfully created sale {SaleId} for customer {CustomerName} with total {Total:C}", 
                    Sale.SaleID, Sale.CustomerName, Sale.TotalAmount);

                // Set success message with invoice link
                TempData["SuccessMessage"] = $"Sale created successfully! Sale ID: #{Sale.SaleID.ToString().Substring(0, 8)} - Total: {Sale.TotalAmount:C}";
                TempData["SaleID"] = Sale.SaleID.ToString();
                TempData["ShowInvoiceLink"] = true;
                
                return RedirectToPage("/Sales/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sale");
                ModelState.AddModelError(string.Empty, "Error creating sale. Please try again.");
                return Page();
            }
        }

        private async Task<bool> ValidateSaleAsync()
        {
            var isValid = true;

            // Validate user authentication
            var userId = ResolveUserId();
            if (userId == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "User authentication required. Please log in again.");
                isValid = false;
            }
            else
            {
                Sale.UserID = userId;
            }

            // Validate and process customer
            isValid &= await ValidateAndProcessCustomerAsync();

            // Validate sale items
            isValid &= ValidateSaleItems();

            // Validate payment information
            isValid &= ValidatePaymentInformation();

            return isValid;
        }

        private async Task<bool> ValidateAndProcessCustomerAsync()
        {
            try
            {
                // Handle existing customer selection
                if (SelectedCustomerID.HasValue && SelectedCustomerID.Value != Guid.Empty)
                {
                    var existingCustomer = await _customerService.GetCustomerByIdAsync(SelectedCustomerID.Value);
                    if (existingCustomer == null)
                    {
                        ModelState.AddModelError(nameof(SelectedCustomerID), "Selected customer not found.");
                        return false;
                    }

                    Sale.CustomerID = existingCustomer.CustomerID;
                    Sale.CustomerName = existingCustomer.CustomerName;
                    return true;
                }

                // Handle customer by name
                if (!string.IsNullOrWhiteSpace(Sale.CustomerName))
                {
                    return await ProcessCustomerByNameAsync();
                }

                // No customer specified - this is allowed for walk-in sales
                Sale.CustomerName = "Walk-in Customer";
                Sale.CustomerID = new Guid("00000000-0000-0000-0000-000000000001"); // Special walk-in customer ID
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating customer");
                ModelState.AddModelError(string.Empty, "Error processing customer information.");
                return false;
            }
        }

        private async Task<bool> ProcessCustomerByNameAsync()
        {
            try
            {
                // Search for existing customer
                var matches = await _customerService.SearchCustomersByNameAsync(Sale.CustomerName);
                var exactMatch = matches.FirstOrDefault(c =>
                    c.CustomerName.Equals(Sale.CustomerName, StringComparison.OrdinalIgnoreCase));

                if (exactMatch != null)
                {
                    Sale.CustomerID = exactMatch.CustomerID;
                    Sale.CustomerName = exactMatch.CustomerName;
                    return true;
                }

                // Create new customer
                var newCustomer = new CustomerDTO
                {
                    CustomerName = Sale.CustomerName.Trim(),
                    ContactNumber = "N/A",
                    Email = string.Empty,
                    Address = string.Empty,
                    CreatedDate = DateTime.UtcNow
                };

                var createdCustomer = await _customerService.CreateCustomerAsync(newCustomer);
                Sale.CustomerID = createdCustomer.CustomerID;
                Sale.CustomerName = createdCustomer.CustomerName;

                _logger.LogInformation("Created new customer: {CustomerName}", createdCustomer.CustomerName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing customer by name: {CustomerName}", Sale.CustomerName);
                ModelState.AddModelError(nameof(Sale.CustomerName), "Error creating customer record.");
                return false;
            }
        }

        private bool ValidateSaleItems()
        {
            if (!Sale.SaleItems.Any())
            {
                ModelState.AddModelError(string.Empty, "At least one item is required for the sale.");
                return false;
            }

            var isValid = true;
            for (int i = 0; i < Sale.SaleItems.Count; i++)
            {
                var item = Sale.SaleItems[i];
                
                if (item.Quantity <= 0)
                {
                    ModelState.AddModelError($"Sale.SaleItems[{i}].Quantity", "Quantity must be greater than zero.");
                    isValid = false;
                }

                if (item.UnitPrice <= 0)
                {
                    ModelState.AddModelError($"Sale.SaleItems[{i}].UnitPrice", "Unit price must be greater than zero.");
                    isValid = false;
                }

                if (item.Discount < 0)
                {
                    ModelState.AddModelError($"Sale.SaleItems[{i}].Discount", "Discount cannot be negative.");
                    isValid = false;
                }

                var lineTotal = item.UnitPrice * item.Quantity;
                if (item.Discount > lineTotal)
                {
                    ModelState.AddModelError($"Sale.SaleItems[{i}].Discount", "Discount cannot exceed line total.");
                    isValid = false;
                }
            }

            return isValid;
        }

        private bool ValidatePaymentInformation()
        {
            var validStatuses = new[] { "Paid", "Pending", "Partial" };
            if (!validStatuses.Contains(Sale.PaymentStatus))
            {
                ModelState.AddModelError(nameof(Sale.PaymentStatus), "Invalid payment status.");
                return false;
            }

            return true;
        }

        private async Task PrepareSaleForCreationAsync()
        {
            Sale.SaleID = Guid.NewGuid();
            Sale.SaleDate = DateTime.UtcNow;
            
            // Ensure payment status is set
            if (string.IsNullOrWhiteSpace(Sale.PaymentStatus))
            {
                Sale.PaymentStatus = "Paid";
            }

            // Ensure all items have valid IDs
            foreach (var item in Sale.SaleItems)
            {
                if (item.SaleItemID == Guid.Empty)
                {
                    item.SaleItemID = Guid.NewGuid();
                }
                
                item.SaleID = Sale.SaleID;
            }

            await Task.CompletedTask;
        }

        // Additional handler methods for enhanced functionality
        public async Task<IActionResult> OnPostSaveAndPrintAsync()
        {
            var result = await OnPostAsync();
            if (result is RedirectToPageResult)
            {
                // If sale was created successfully, redirect to invoice for printing
                TempData["SuccessMessage"] = $"Sale created successfully! Sale ID: #{Sale.SaleID.ToString().Substring(0, 8)} - Total: {Sale.TotalAmount:C}";
                return RedirectToPage("Invoice", new { id = Sale.SaleID });
            }
            return result;
        }

        public async Task<IActionResult> OnPostSaveAndNewAsync()
        {
            var result = await OnPostAsync();
            if (result is RedirectToPageResult)
            {
                // If sale was created successfully, redirect to create new sale
                TempData["SuccessMessage"] = $"Sale created successfully! Sale ID: #{Sale.SaleID.ToString().Substring(0, 8)} - Total: {Sale.TotalAmount:C}. Ready for next sale.";
                return RedirectToPage("Create");
            }
            return result;
        }

        // API endpoints for real-time validation
        public async Task<IActionResult> OnGetProductStockAsync(Guid productId)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                    return new JsonResult(new { success = false, message = "Product not found" });

                return new JsonResult(new 
                { 
                    success = true, 
                    stock = product.TotalStock,
                    price = product.UnitPrice,
                    name = product.ProductName,
                    isActive = product.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product stock for {ProductId}", productId);
                return new JsonResult(new { success = false, message = "Error retrieving product information" });
            }
        }

        public async Task<IActionResult> OnGetSearchCustomersAsync(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                    return new JsonResult(new { success = true, customers = new object[0] });

                var customers = await _customerService.SearchCustomersByNameAsync(term);
                var result = customers.Take(10).Select(c => new 
                {
                    id = c.CustomerID,
                    name = c.CustomerName,
                    contact = c.ContactNumber,
                    email = c.Email
                });

                return new JsonResult(new { success = true, customers = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers with term {Term}", term);
                return new JsonResult(new { success = false, message = "Error searching customers" });
            }
        }

        public async Task<IActionResult> OnGetValidateSaleAsync()
        {
            try
            {
                // Prepare sale data
                if (Sale?.SaleItems == null || !Sale.SaleItems.Any())
                    return new JsonResult(new { success = false, message = "No items in sale" });

                // Validate stock for all items sequentially to avoid DbContext concurrency issues
                var stockValidation = new List<object>();
                var totalValid = true;

                foreach (var item in Sale.SaleItems)
                {
                    var isValid = await _salesService.ValidateProductStockAsync(item.ProductID, item.Quantity);
                    if (!isValid) totalValid = false;

                    var product = await _productService.GetProductByIdAsync(item.ProductID);
                    stockValidation.Add(new 
                    {
                        productId = item.ProductID,
                        productName = item.ProductName,
                        requested = item.Quantity,
                        available = product?.TotalStock ?? 0,
                        isValid = isValid
                    });
                }

                // Calculate total and check if sale can be processed
                var canProcess = await _salesService.CanProcessSaleAsync(Sale);
                var total = await _salesService.CalculateSaleTotalAsync(Sale);

                return new JsonResult(new 
                { 
                    success = totalValid, 
                    canProcess = canProcess,
                    validation = stockValidation,
                    total = total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating sale");
                return new JsonResult(new { success = false, message = "Error validating sale" });
            }
        }
    }
}