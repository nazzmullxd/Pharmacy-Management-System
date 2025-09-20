using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Business.Interfaces;
using Business.DTO;
using System;
using System.Threading.Tasks;

namespace Web.Pages.Customers
{
    public class EditModel : PageModel
    {
        private readonly ICustomerService _customerService;

        public EditModel(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [FromRoute]
        public string Id { get; set; } = string.Empty;

        [BindProperty]
        public CustomerDTO Customer { get; set; } = new CustomerDTO();

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

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
                var customer = await _customerService.GetCustomerByIdAsync(customerId);
                if (customer == null)
                {
                    ErrorMessage = $"Customer with ID {id} not found.";
                    return Page();
                }

                Customer = customer;
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading customer: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Validate email uniqueness (excluding current customer)
                var isEmailUnique = await _customerService.IsEmailUniqueAsync(Customer.Email, Customer.CustomerID);
                if (!isEmailUnique)
                {
                    ModelState.AddModelError("Customer.Email", "This email address is already in use by another customer.");
                    return Page();
                }

                // Validate contact number uniqueness (excluding current customer)
                var isContactUnique = await _customerService.IsContactNumberUniqueAsync(Customer.ContactNumber, Customer.CustomerID);
                if (!isContactUnique)
                {
                    ModelState.AddModelError("Customer.ContactNumber", "This contact number is already in use by another customer.");
                    return Page();
                }

                var updatedCustomer = await _customerService.UpdateCustomerAsync(Customer);
                
                SuccessMessage = "Customer updated successfully!";
                TempData["SuccessMessage"] = SuccessMessage;
                
                return RedirectToPage("/Customers/Details", new { id = Customer.CustomerID });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating customer: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            if (!Guid.TryParse(Id, out Guid customerId))
            {
                TempData["ErrorMessage"] = "Invalid customer ID format.";
                return RedirectToPage("/Customers/Index");
            }

            try
            {
                var success = await _customerService.DeleteCustomerAsync(customerId);
                if (success)
                {
                    TempData["SuccessMessage"] = "Customer deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete customer. Customer may not exist.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting customer: {ex.Message}";
            }

            return RedirectToPage("/Customers/Index");
        }
    }
}


