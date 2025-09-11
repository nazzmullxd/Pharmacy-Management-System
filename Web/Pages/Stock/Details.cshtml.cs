using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Stock
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
        public IEnumerable<ProductBatchDTO> Batches { get; set; } = Enumerable.Empty<ProductBatchDTO>();

        public async Task OnGet(Guid id)
        {
            Id = id;
            Product = await _productService.GetProductByIdAsync(id);
            if (Product != null)
            {
                Batches = await _stockService.GetBatchesByProductAsync(id);
            }
        }
    }
}


