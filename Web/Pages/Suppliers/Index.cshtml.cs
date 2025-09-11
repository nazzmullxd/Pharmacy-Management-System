using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Suppliers
{
    public class IndexModel : PageModel
    {
        private readonly ISupplierService _supplierService;

        public IndexModel(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        public IEnumerable<SupplierDTO> Suppliers { get; set; } = Enumerable.Empty<SupplierDTO>();

        public async Task OnGet()
        {
            Suppliers = await _supplierService.GetAllSuppliersAsync();
        }
    }
}


