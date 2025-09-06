using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Customers
{
    public class IndexModel : PageModel
    {
        private readonly ICustomerService _customerService;
        private readonly ISalesService _salesService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ICustomerService customerService, ISalesService salesService, ILogger<IndexModel> logger)
        {
            _customerService = customerService;
            _salesService = salesService;
            _logger = logger;
        }

        public IEnumerable<CustomerDTO> Customers { get; set; } = new List<CustomerDTO>();
        public IEnumerable<CustomerDTO> TopCustomers { get; set; } = new List<CustomerDTO>();
        public IEnumerable<CustomerDTO> RecentCustomers { get; set; } = new List<CustomerDTO>();
        
        // Summary properties
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int NewCustomers { get; set; }
        public decimal TotalCustomerSales { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Load all customers
                Customers = await _customerService.GetAllCustomersAsync();
                
                // Load recent customers (last 30 days)
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                RecentCustomers = Customers.Where(c => c.CreatedDate >= thirtyDaysAgo)
                                          .OrderByDescending(c => c.CreatedDate)
                                          .Take(10);
                
                // Calculate summary statistics
                await CalculateSummaryStatistics();
                
                // Load top customers (this month)
                await LoadTopCustomers();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customers data");
                Customers = new List<CustomerDTO>();
                TopCustomers = new List<CustomerDTO>();
                RecentCustomers = new List<CustomerDTO>();
            }
        }

        private async Task CalculateSummaryStatistics()
        {
            try
            {
                TotalCustomers = Customers.Count();
                
                // Calculate active customers (customers with purchases in last 30 days)
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                ActiveCustomers = Customers.Count(c => c.LastPurchaseDate.HasValue && c.LastPurchaseDate >= thirtyDaysAgo);
                
                // Calculate new customers (registered in last 30 days)
                NewCustomers = Customers.Count(c => c.CreatedDate >= thirtyDaysAgo);
                
                // Calculate total customer sales
                TotalCustomerSales = Customers.Sum(c => c.TotalPurchases ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating customer summary statistics");
                TotalCustomers = 0;
                ActiveCustomers = 0;
                NewCustomers = 0;
                TotalCustomerSales = 0;
            }
        }

        private async Task LoadTopCustomers()
        {
            try
            {
                // Get top customers by total purchases this month
                var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var monthEnd = monthStart.AddMonths(1);
                
                // In a real implementation, this would come from a dedicated service method
                TopCustomers = Customers
                    .Where(c => c.LastPurchaseDate.HasValue && c.LastPurchaseDate >= monthStart)
                    .OrderByDescending(c => c.TotalPurchases ?? 0)
                    .Take(10);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading top customers");
                TopCustomers = new List<CustomerDTO>();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid customerId)
        {
            try
            {
                await _customerService.DeleteCustomerAsync(customerId);
                TempData["SuccessMessage"] = "Customer deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {CustomerId}", customerId);
                TempData["ErrorMessage"] = "Error deleting customer. Please try again.";
            }

            return RedirectToPage();
        }
    }
}
