using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Business.Interfaces;
using Business.DTO;
using System;
using System.Threading.Tasks;

namespace Web.Pages.Customers
{
    public class DetailsModel : PageModel
    {
        private readonly ICustomerService _customerService;

        public DetailsModel(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [FromRoute]
        public string Id { get; set; } = string.Empty;

        public CustomerDTO? Customer { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            Id = id;

            if (!Guid.TryParse(id, out Guid customerId))
            {
                ErrorMessage = "Invalid customer ID format.";
                return Page();
            }

            try
            {
                Customer = await _customerService.GetCustomerByIdAsync(customerId);
                if (Customer == null)
                {
                    ErrorMessage = $"Customer with ID {id} not found.";
                    return Page();
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading customer: {ex.Message}";
                return Page();
            }
        }
    }
}


