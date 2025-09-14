using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Sales
{
    public class CreateModel : PageModel
    {
        private readonly ISalesService _salesService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;

        public CreateModel(ISalesService salesService, ICustomerService customerService, IProductService productService)
        {
            _salesService = salesService;
            _customerService = customerService;
            _productService = productService;
        }

        public IEnumerable<ProductDTO> Products { get; set; } = Enumerable.Empty<ProductDTO>();

        [BindProperty]
        public SaleDTO Sale { get; set; } = new SaleDTO();

        public async Task OnGet()
        {
            Products = await _productService.GetAllProductsAsync();
            // Preselect customer if provided
            if (Request.Query.ContainsKey("customerId") && Guid.TryParse(Request.Query["customerId"], out var cid))
            {
                Sale.CustomerID = cid;
            }
        }

        public async Task<IActionResult> OnPostAddItemAsync(Guid productId, int quantity)
        {
            Products = await _productService.GetAllProductsAsync();
            if (Sale == null)
                Sale = new SaleDTO();
            if (productId != Guid.Empty && quantity > 0)
            {
                var product = Products.FirstOrDefault(p => p.ProductID == productId);
                var unitPrice = product?.UnitPrice ?? 0m;
                Sale.SaleItems.Add(new SaleItemDTO
                {
                    SaleItemID = Guid.NewGuid(),
                    ProductID = productId,
                    ProductName = product?.ProductName ?? string.Empty,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = unitPrice * quantity
                });
                Sale.TotalAmount = Sale.SaleItems.Sum(i => i.TotalPrice);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            Products = await _productService.GetAllProductsAsync();
            if (Sale == null || string.IsNullOrWhiteSpace(Sale.CustomerName) || !Sale.SaleItems.Any())
            {
                ModelState.AddModelError(string.Empty, "Customer name and at least one item are required.");
                return Page();
            }
            // Try to find customer by name
            var customers = await _customerService.SearchCustomersByNameAsync(Sale.CustomerName);
            var customer = customers.FirstOrDefault(c => c.CustomerName.Equals(Sale.CustomerName, StringComparison.OrdinalIgnoreCase));
            if (customer != null)
            {
                Sale.CustomerID = customer.CustomerID;
            }
            else
            {
                // Create minimal customer record
                var newCustomer = new CustomerDTO
                {
                    CustomerName = Sale.CustomerName,
                    ContactNumber = string.Empty,
                    Email = string.Empty,
                    Address = string.Empty
                };
                var createdCustomer = await _customerService.CreateCustomerAsync(newCustomer);
                Sale.CustomerID = createdCustomer.CustomerID;
            }
            Sale.SaleID = Guid.NewGuid();
            Sale.SaleDate = DateTime.UtcNow;
            Sale.PaymentStatus = "Paid";
            await _salesService.CreateSaleAsync(Sale);
            TempData["Message"] = "Sale created.";
            return RedirectToPage("Index");
        }
    }
}


