using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Api
{
    public class CustomersSearchModel : PageModel
    {
        private readonly ICustomerService _customerService;
        public CustomersSearchModel(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> OnGetAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new JsonResult(Array.Empty<object>());
            var customers = await _customerService.SearchCustomersByNameAsync(name);
            var result = customers
                .Select(c => new { customerID = c.CustomerID, customerName = c.CustomerName })
                .Take(10)
                .ToList();
            return new JsonResult(result);
        }
    }
}
