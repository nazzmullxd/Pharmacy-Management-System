using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IStockService _stockService;

        public DetailsModel(IProductService productService, IStockService stockService)
        {
            _productService = productService;
            _stockService = stockService;
        }

        [FromRoute]
        public Guid Id { get; set; }

        public ProductDTO? Product { get; set; }

        public async Task OnGet(Guid id)
        {
            Id = id;
            Product = await _productService.GetProductByIdAsync(id);
            if (Product != null)
            {
                // Always fetch latest stock from batches
                Product.TotalStock = await _stockService.GetTotalStockForProductAsync(Product.ProductID);
            }
        }
    }
}