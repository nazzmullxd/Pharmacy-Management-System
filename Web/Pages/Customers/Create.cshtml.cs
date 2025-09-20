using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Web.Pages.Customers
{
    public class CreateModel : PageModel
    {
        private readonly ICustomerService _customerService;

        public CreateModel(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [BindProperty]
        public CustomerDTO Customer { get; set; } = new CustomerDTO();
        [BindProperty]
        public PurchaseOrderDTO Order { get; set; } = new PurchaseOrderDTO();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            Customer.CustomerID = Guid.NewGuid();

            // Set CreatedBy only if the user is authenticated
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                    Customer.CreatedBy = Guid.Parse(userId);
                else
                    Customer.CreatedBy = Guid.Empty; // or handle as needed
            }
            else
            {
                Customer.CreatedBy = Guid.Empty; // or handle as needed
            }

            await _customerService.CreateCustomerAsync(Customer);
            return RedirectToPage("Index");
        }
    }
}



