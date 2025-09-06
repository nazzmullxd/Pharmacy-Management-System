using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IProductService productService, ILogger<IndexModel> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public IEnumerable<ProductDTO> Products { get; set; } = new List<ProductDTO>();

        public async Task OnGetAsync()
        {
            try
            {
                Products = await _productService.GetAllProductsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                // In a real application, you might want to show an error message to the user
                Products = new List<ProductDTO>();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid productId)
        {
            try
            {
                await _productService.DeleteProductAsync(productId);
                TempData["SuccessMessage"] = "Product deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", productId);
                TempData["ErrorMessage"] = "Error deleting product. Please try again.";
            }

            return RedirectToPage();
        }
    }
}
