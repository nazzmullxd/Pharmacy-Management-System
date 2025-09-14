using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Stock
{
    public class AddBatchModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ISupplierService _supplierService;
        private readonly IStockService _stockService;

        public AddBatchModel(IProductService productService, ISupplierService supplierService, IStockService stockService)
        {
            _productService = productService;
            _supplierService = supplierService;
            _stockService = stockService;
        }

        [BindProperty]
        public ProductBatchDTO Batch { get; set; } = new ProductBatchDTO();
        public IEnumerable<ProductDTO> Products { get; set; } = Enumerable.Empty<ProductDTO>();
        public IEnumerable<SupplierDTO> Suppliers { get; set; } = Enumerable.Empty<SupplierDTO>();

        public async Task OnGet()
        {
            Products = await _productService.GetAllProductsAsync();
            Suppliers = await _supplierService.GetAllSuppliersAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Products = await _productService.GetAllProductsAsync();
            Suppliers = await _supplierService.GetAllSuppliersAsync();
            if (!ModelState.IsValid)
                return Page();
            await _stockService.CreateProductBatchAsync(Batch);
            TempData["Message"] = "Batch added successfully.";
            return RedirectToPage("Batches");
        }
    }
}