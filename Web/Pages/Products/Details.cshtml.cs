using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly IProductService _productService;

        public DetailsModel(IProductService productService)
        {
            _productService = productService;
        }

        [FromRoute]
        public Guid Id { get; set; }

        public ProductDTO? Product { get; set; }

        public async Task OnGet(Guid id)
        {
            Id = id;
            Product = await _productService.GetProductByIdAsync(id);
        }
    }
}


