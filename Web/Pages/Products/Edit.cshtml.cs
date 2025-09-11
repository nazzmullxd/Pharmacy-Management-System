using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Products
{
    public class EditModel : PageModel
    {
        private readonly IProductService _productService;

        public EditModel(IProductService productService)
        {
            _productService = productService;
        }

        [FromRoute]
        public Guid Id { get; set; }

        [BindProperty]
        public ProductDTO Product { get; set; } = new ProductDTO();

        public async Task OnGet(Guid id)
        {
            Id = id;
            var existing = await _productService.GetProductByIdAsync(id);
            if (existing != null)
            {
                Product = existing;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (Product.ProductID == Guid.Empty)
                Product.ProductID = Id;

            await _productService.UpdateProductAsync(Product);
            return RedirectToPage("Index");
        }
    }
}


