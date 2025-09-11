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

        public IEnumerable<CustomerDTO> Customers { get; set; } = Enumerable.Empty<CustomerDTO>();
        public IEnumerable<ProductDTO> Products { get; set; } = Enumerable.Empty<ProductDTO>();

        [BindProperty]
        public SaleDTO Sale { get; set; } = new SaleDTO();

        public async Task OnGet()
        {
            Customers = await _customerService.GetAllCustomersAsync();
            Products = await _productService.GetAllProductsAsync();

            // Preselect customer if provided
            if (Request.Query.ContainsKey("customerId") && Guid.TryParse(Request.Query["customerId"], out var cid))
            {
                Sale.CustomerID = cid;
            }
        }

        public async Task<IActionResult> OnPostAddItemAsync(Guid productId, int quantity)
        {
            Customers = await _customerService.GetAllCustomersAsync();
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
            Customers = await _customerService.GetAllCustomersAsync();
            Products = await _productService.GetAllProductsAsync();

            if (Sale == null || Sale.CustomerID == Guid.Empty || !Sale.SaleItems.Any())
            {
                ModelState.AddModelError(string.Empty, "Customer and at least one item are required.");
                return Page();
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


