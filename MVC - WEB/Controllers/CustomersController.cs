using Microsoft.AspNetCore.Mvc;
using Business.Interfaces;
using Business.DTO;
using MVC_WEB.Filters;
using MVC_WEB.Models.ViewModels;

namespace MVC_WEB.Controllers
{
    [Authenticated]
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                var customerList = customers.ToList();
                
                // Calculate statistics
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                
                var viewModel = new CustomerListViewModel
                {
                    Customers = customerList,
                    TotalCustomers = customerList.Count,
                    ActiveCustomers = customerList.Count(c => c.LastPurchaseDate.HasValue && c.LastPurchaseDate >= thirtyDaysAgo),
                    NewCustomers = customerList.Count(c => c.CreatedDate >= thirtyDaysAgo),
                    TotalCustomerSales = customerList.Sum(c => c.TotalPurchases ?? 0),
                    RecentCustomers = customerList.Where(c => c.CreatedDate >= thirtyDaysAgo)
                                                  .OrderByDescending(c => c.CreatedDate)
                                                  .Take(10)
                                                  .ToList(),
                    TopCustomers = customerList.OrderByDescending(c => c.TotalPurchases ?? 0)
                                              .Take(5)
                                              .ToList()
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customers list");
                TempData["Error"] = "Failed to load customers. Please try again.";
                return View(new CustomerListViewModel());
            }
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new CustomerDetailsViewModel
                {
                    Customer = customer
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer details for {CustomerId}", id);
                TempData["Error"] = "Failed to load customer details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Customers/Create
        [AdminOnly]
        public IActionResult Create()
        {
            return View(new CustomerCreateViewModel());
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Create(CustomerCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Validate email uniqueness
                if (!string.IsNullOrEmpty(model.Email))
                {
                    var isEmailUnique = await _customerService.IsEmailUniqueAsync(model.Email);
                    if (!isEmailUnique)
                    {
                        ModelState.AddModelError("Email", "This email address is already in use.");
                        return View(model);
                    }
                }

                // Validate contact number uniqueness
                var isContactUnique = await _customerService.IsContactNumberUniqueAsync(model.ContactNumber);
                if (!isContactUnique)
                {
                    ModelState.AddModelError("ContactNumber", "This contact number is already in use.");
                    return View(model);
                }

                // Get user ID from session
                var userIdString = HttpContext.Session.GetString("userId");
                Guid createdBy = Guid.Empty;
                if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var userId))
                {
                    createdBy = userId;
                }

                var customerDto = new CustomerDTO
                {
                    CustomerID = Guid.NewGuid(),
                    CustomerName = model.CustomerName,
                    ContactNumber = model.ContactNumber,
                    Email = model.Email ?? string.Empty,
                    Address = model.Address ?? string.Empty,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = createdBy
                };

                await _customerService.CreateCustomerAsync(customerDto);
                
                TempData["Success"] = $"Customer '{model.CustomerName}' created successfully.";
                _logger.LogInformation("Customer created: {CustomerName}", model.CustomerName);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                ModelState.AddModelError("", "Failed to create customer. Please try again.");
                return View(model);
            }
        }

        // GET: Customers/Edit/5
        [AdminOnly]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new CustomerEditViewModel
                {
                    CustomerID = customer.CustomerID,
                    CustomerName = customer.CustomerName,
                    ContactNumber = customer.ContactNumber,
                    Email = customer.Email,
                    Address = customer.Address,
                    CreatedDate = customer.CreatedDate,
                    TotalPurchases = customer.TotalPurchases,
                    LastPurchaseDate = customer.LastPurchaseDate
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer for editing {CustomerId}", id);
                TempData["Error"] = "Failed to load customer. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Edit(Guid id, CustomerEditViewModel model)
        {
            if (id != model.CustomerID)
            {
                TempData["Error"] = "Invalid customer ID.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Validate email uniqueness (excluding current customer)
                if (!string.IsNullOrEmpty(model.Email))
                {
                    var isEmailUnique = await _customerService.IsEmailUniqueAsync(model.Email, model.CustomerID);
                    if (!isEmailUnique)
                    {
                        ModelState.AddModelError("Email", "This email address is already in use by another customer.");
                        return View(model);
                    }
                }

                // Validate contact number uniqueness (excluding current customer)
                var isContactUnique = await _customerService.IsContactNumberUniqueAsync(model.ContactNumber, model.CustomerID);
                if (!isContactUnique)
                {
                    ModelState.AddModelError("ContactNumber", "This contact number is already in use by another customer.");
                    return View(model);
                }

                var existingCustomer = await _customerService.GetCustomerByIdAsync(id);
                if (existingCustomer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction(nameof(Index));
                }

                existingCustomer.CustomerName = model.CustomerName;
                existingCustomer.ContactNumber = model.ContactNumber;
                existingCustomer.Email = model.Email ?? string.Empty;
                existingCustomer.Address = model.Address ?? string.Empty;

                await _customerService.UpdateCustomerAsync(existingCustomer);
                
                TempData["Success"] = $"Customer '{model.CustomerName}' updated successfully.";
                _logger.LogInformation("Customer updated: {CustomerId} ({CustomerName})", id, model.CustomerName);
                
                return RedirectToAction(nameof(Details), new { id = model.CustomerID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId}", id);
                ModelState.AddModelError("", "Failed to update customer. Please try again.");
                return View(model);
            }
        }

        // GET: Customers/Delete/5
        [AdminOnly]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new CustomerDeleteViewModel
                {
                    Customer = customer
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer for deletion {CustomerId}", id);
                TempData["Error"] = "Failed to load customer. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _customerService.DeleteCustomerAsync(id);
                if (result)
                {
                    TempData["Success"] = $"Customer '{customer.CustomerName}' deleted successfully.";
                    _logger.LogInformation("Customer deleted: {CustomerId} ({CustomerName})", id, customer.CustomerName);
                }
                else
                {
                    TempData["Error"] = "Failed to delete customer.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
                TempData["Error"] = "Failed to delete customer. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Customers/Search
        public async Task<IActionResult> Search(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return RedirectToAction(nameof(Index));
                }

                var customers = await _customerService.SearchCustomersByNameAsync(query);
                var customerList = customers.ToList();
                
                // Also search by contact number
                var contactResults = await _customerService.GetCustomersByContactNumberAsync(query);
                var combined = customerList.Union(contactResults).Distinct().ToList();

                var viewModel = new CustomerListViewModel
                {
                    Customers = combined,
                    TotalCustomers = combined.Count,
                    SearchQuery = query
                };

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers with query: {Query}", query);
                TempData["Error"] = "Failed to search customers. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Customers/DeleteAjax (for AJAX delete from Index page)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> DeleteAjax(Guid id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer not found." });
                }

                var result = await _customerService.DeleteCustomerAsync(id);
                if (result)
                {
                    _logger.LogInformation("Customer deleted via AJAX: {CustomerId} ({CustomerName})", id, customer.CustomerName);
                    return Json(new { success = true, message = $"Customer '{customer.CustomerName}' deleted successfully." });
                }
                
                return Json(new { success = false, message = "Failed to delete customer." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer via AJAX {CustomerId}", id);
                return Json(new { success = false, message = "An error occurred while deleting the customer." });
            }
        }
    }
}
