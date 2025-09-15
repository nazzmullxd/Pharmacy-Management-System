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
            _salesService = salesService;
            _customerService = customerService;
            _productService = productService;
            _logger = logger;
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

        public async Task OnGet()
        {
            await LoadReferenceDataAsync();

            if (Request.Query.TryGetValue("customerId", out var value) &&
                Guid.TryParse(value, out var cid))
            {
                SelectedCustomerID = cid;
                var c = await _customerService.GetCustomerByIdAsync(cid);
                if (c != null)
                {
                    Sale.CustomerID = c.CustomerID;
                    Sale.CustomerName = c.CustomerName;
                }
            }
        }

        private async Task LoadReferenceDataAsync()
        {
            try
            {
                Products = await _productService.GetAllProductsAsync();
                Customers = await _customerService.GetAllCustomersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reference data");
                ModelState.AddModelError(string.Empty, "Error loading product or customer data");
            }
        }

        private void RecomputeTotals()
        {
            Sale.SaleItems ??= new List<SaleItemDTO>();
            foreach (var li in Sale.SaleItems)
            {
                if (string.IsNullOrWhiteSpace(li.BatchNumber))
                    li.BatchNumber = "N/A";
                li.RecomputeTotal();
            }
            Sale.TotalAmount = Sale.SaleItems.Sum(i => i.TotalPrice);
        }

        private Guid ResolveUserId()
        {
            try
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null && Guid.TryParse(claim.Value, out var id)) return id;

                var sessionUser = HttpContext.Session.GetString("UserId");
                if (!string.IsNullOrWhiteSpace(sessionUser) && Guid.TryParse(sessionUser, out var sid))
                    return sid;

                return Guid.Empty;
            }
            catch
            {
                return Guid.Empty;
            }
        }

        public async Task<IActionResult> OnPostAddItemAsync()
        {
            await LoadReferenceDataAsync();
            Sale.SaleItems ??= new List<SaleItemDTO>();

            if (ProductIdToAdd is null || ProductIdToAdd == Guid.Empty)
                ModelState.AddModelError(nameof(ProductIdToAdd), "Select a product.");

            if (QuantityToAdd <= 0)
                ModelState.AddModelError(nameof(QuantityToAdd), "Quantity must be greater than zero.");

            if (!ModelState.IsValid)
            {
                RecomputeTotals();
                return Page();
            }

            var product = ProductIdToAdd.HasValue
                ? Products.FirstOrDefault(p => p.ProductID == ProductIdToAdd.Value)
                : null;

            if (product == null)
            {
                ModelState.AddModelError(nameof(ProductIdToAdd), "Product not found.");
                RecomputeTotals();
                return Page();
            }

            // Check if product already exists in sale items
            var existingItem = Sale.SaleItems.FirstOrDefault(i => i.ProductID == product.ProductID);
            if (existingItem != null)
            {
                // Update quantity if product already exists
                existingItem.Quantity += QuantityToAdd;
                existingItem.RecomputeTotal();
            }
            else
            {
                // Add new item
                Sale.SaleItems.Add(new SaleItemDTO
                {
                    SaleItemID = Guid.NewGuid(),
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    Quantity = QuantityToAdd,
                    UnitPrice = product.UnitPrice,
                    Discount = 0m,
                    BatchNumber = "N/A"
                });
            }

            RecomputeTotals();

            // Reset form fields
            ProductIdToAdd = null;
            QuantityToAdd = 1;

            // Clear model state for these fields only
            ModelState.Remove(nameof(ProductIdToAdd));
            ModelState.Remove(nameof(QuantityToAdd));

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveItemAsync(int index)
        {
            await LoadReferenceDataAsync();
            if (Sale?.SaleItems != null && index >= 0 && index < Sale.SaleItems.Count)
            {
                Sale.SaleItems.RemoveAt(index);
            }
            RecomputeTotals();

            // Clear model state to prevent validation issues
            ModelState.Clear();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadReferenceDataAsync();
            Sale.SaleItems ??= new List<SaleItemDTO>();

            // Clear add-item related model state
            ModelState.Remove(nameof(ProductIdToAdd));
            ModelState.Remove(nameof(QuantityToAdd));

            RecomputeTotals();

            // Validate user
            var userId = ResolveUserId();
            if (userId == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "User authentication required. Please log in again.");
                return Page();
            }
            Sale.UserID = userId;

            // Validate customer
            if (SelectedCustomerID.HasValue && SelectedCustomerID.Value != Guid.Empty)
            {
                var existing = await _customerService.GetCustomerByIdAsync(SelectedCustomerID.Value);
                if (existing == null)
                {
                    ModelState.AddModelError(string.Empty, "Selected customer not found.");
                }
                else
                {
                    Sale.CustomerID = existing.CustomerID;
                    Sale.CustomerName = existing.CustomerName;
                }
            }

            // Handle customer by name if no ID selected
            if (Sale.CustomerID == Guid.Empty && !string.IsNullOrWhiteSpace(Sale.CustomerName))
            {
                try
                {
                    var matches = await _customerService.SearchCustomersByNameAsync(Sale.CustomerName);
                    var match = matches.FirstOrDefault(c =>
                        c.CustomerName.Equals(Sale.CustomerName, StringComparison.OrdinalIgnoreCase));

                    if (match != null)
                    {
                        Sale.CustomerID = match.CustomerID;
                        Sale.CustomerName = match.CustomerName;
                    }
                    else
                    {
                        var created = await _customerService.CreateCustomerAsync(new CustomerDTO
                        {
                            CustomerName = Sale.CustomerName,
                            ContactNumber = "N/A",
                            Email = string.Empty,
                            Address = string.Empty,
                            CreatedDate = DateTime.UtcNow
                        });
                        Sale.CustomerID = created.CustomerID;
                        Sale.CustomerName = created.CustomerName;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating customer");
                    ModelState.AddModelError(string.Empty, "Error creating customer: " + ex.Message);
                }
            }

            // Final validation
            if (Sale.CustomerID == Guid.Empty)
                ModelState.AddModelError(string.Empty, "Customer is required.");

            if (!Sale.SaleItems.Any())
                ModelState.AddModelError(string.Empty, "At least one item is required.");

            // Validate each sale item
            for (int i = 0; i < Sale.SaleItems.Count; i++)
            {
                var item = Sale.SaleItems[i];
                if (item.Quantity <= 0)
                    ModelState.AddModelError($"Sale.SaleItems[{i}].Quantity", "Quantity must be greater than zero.");

                if (item.UnitPrice <= 0)
                    ModelState.AddModelError($"Sale.SaleItems[{i}].UnitPrice", "Unit price must be greater than zero.");

                if (item.Discount < 0)
                    ModelState.AddModelError($"Sale.SaleItems[{i}].Discount", "Discount cannot be negative.");

                if (item.Discount > item.UnitPrice * item.Quantity)
                    ModelState.AddModelError($"Sale.SaleItems[{i}].Discount", "Discount cannot exceed line total.");
            }

            if (!ModelState.IsValid)
            {
                // Log validation errors for debugging
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Validation error: {ErrorMessage}", error.ErrorMessage);
                }
                return Page();
            }

            // Set final sale properties
            Sale.SaleID = Guid.NewGuid();
            Sale.SaleDate = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(Sale.PaymentStatus))
                Sale.PaymentStatus = "Paid";

            try
            {
                await _salesService.CreateSaleAsync(Sale);
                TempData["SuccessMessage"] = "Sale created successfully.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sale");
                ModelState.AddModelError(string.Empty, $"Error creating sale: {ex.Message}");
                return Page();
            }
        }
    }
}