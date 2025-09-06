using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IProductService productService, ILogger<CreateModel> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [BindProperty]
        public ProductDTO Product { get; set; } = new ProductDTO();

        public void OnGet()
        {
            // Initialize default values
            Product.IsActive = true;
            Product.LowStockThreshold = 10;
            Product.CreatedDate = DateTime.UtcNow;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Validate business rules
                if (Product.UnitPrice <= 0)
                {
                    ModelState.AddModelError("Product.UnitPrice", "Unit price must be greater than 0.");
                    return Page();
                }

                if (Product.DefaultRetailPrice <= Product.UnitPrice)
                {
                    ModelState.AddModelError("Product.DefaultRetailPrice", "Retail price must be greater than unit price.");
                    return Page();
                }

                if (Product.DefaultWholeSalePrice <= Product.UnitPrice)
                {
                    ModelState.AddModelError("Product.DefaultWholeSalePrice", "Wholesale price must be greater than unit price.");
                    return Page();
                }

                // Check if barcode is unique (if provided)
                if (!string.IsNullOrEmpty(Product.Barcode))
                {
                    var isUnique = await _productService.IsBarcodeUniqueAsync(Product.Barcode);
                    if (!isUnique)
                    {
                        ModelState.AddModelError("Product.Barcode", "This barcode is already in use.");
                        return Page();
                    }
                }

                // Set additional properties
                Product.ProductID = Guid.NewGuid();
                Product.CreatedDate = DateTime.UtcNow;
                Product.TotalStock = 0; // New products start with 0 stock

                await _productService.CreateProductAsync(Product);

                TempData["SuccessMessage"] = $"Product '{Product.ProductName}' created successfully.";
                return RedirectToPage("/Products/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product {ProductName}", Product.ProductName);
                ModelState.AddModelError(string.Empty, "An error occurred while creating the product. Please try again.");
                return Page();
            }
        }
    }
}
