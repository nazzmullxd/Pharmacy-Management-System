using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Suppliers
{
    public class CreateModel : PageModel
    {
        private readonly ISupplierService _supplierService;

        public CreateModel(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [BindProperty]
        public SupplierDTO Supplier { get; set; } = new SupplierDTO();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            await _supplierService.CreateSupplierAsync(Supplier);
            return RedirectToPage("Index");
        }
    }
}


