using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

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

            // Set default values for missing fields
            Supplier.CreatedDate = DateTime.Now;
            Supplier.UpdatedDate = DateTime.Now;
            Supplier.CreatedBy = "System User"; // In a real app, this should be the current user's ID/name
            Supplier.IsActive = true;

            try
            {
                await _supplierService.CreateSupplierAsync(Supplier);
                TempData["SuccessMessage"] = $"Supplier '{Supplier.SupplierName}' has been created successfully!";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating supplier: {ex.Message}");
                return Page();
            }
        }
    }
}


