using Microsoft.AspNetCore.Mvc;
using Business.Interfaces;
using Business.DTO;
using MVC_WEB.Filters;
using MVC_WEB.Models.ViewModels;

namespace MVC_WEB.Controllers
{
    [Authenticated]
    public class SuppliersController : Controller
    {
        private readonly ISupplierService _supplierService;
        private readonly ILogger<SuppliersController> _logger;

        public SuppliersController(ISupplierService supplierService, ILogger<SuppliersController> logger)
        {
            _supplierService = supplierService;
            _logger = logger;
        }

        // GET: Suppliers
        public async Task<IActionResult> Index()
        {
            try
            {
                var suppliers = await _supplierService.GetAllSuppliersAsync();
                var supplierList = suppliers.ToList();
                
                var viewModel = new SupplierListViewModel
                {
                    Suppliers = supplierList,
                    TotalSuppliers = supplierList.Count,
                    ActiveSuppliers = supplierList.Count(s => s.IsActive),
                    InactiveSuppliers = supplierList.Count(s => !s.IsActive),
                    CompleteInfoCount = supplierList.Count(s => !string.IsNullOrEmpty(s.Email) && !string.IsNullOrEmpty(s.PhoneNumber))
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading suppliers list");
                TempData["Error"] = "Failed to load suppliers. Please try again.";
                return View(new SupplierListViewModel());
            }
        }

        // GET: Suppliers/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(id);
                if (supplier == null)
                {
                    TempData["Error"] = "Supplier not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new SupplierDetailsViewModel
                {
                    Supplier = supplier
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading supplier details for {SupplierId}", id);
                TempData["Error"] = "Failed to load supplier details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Suppliers/Create
        [AdminOnly]
        public IActionResult Create()
        {
            return View(new SupplierCreateViewModel { IsActive = true });
        }

        // POST: Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Create(SupplierCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Validate supplier name uniqueness
                var isNameUnique = await _supplierService.IsSupplierNameUniqueAsync(model.SupplierName);
                if (!isNameUnique)
                {
                    ModelState.AddModelError("SupplierName", "A supplier with this name already exists.");
                    return View(model);
                }

                // Get user info from session
                var userName = HttpContext.Session.GetString("userName") ?? "System";

                var supplierDto = new SupplierDTO
                {
                    SupplierID = Guid.NewGuid(),
                    SupplierName = model.SupplierName,
                    ContactPerson = model.ContactPerson ?? string.Empty,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    Address = model.Address ?? string.Empty,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    CreatedBy = userName,
                    IsActive = model.IsActive
                };

                await _supplierService.CreateSupplierAsync(supplierDto);
                
                TempData["Success"] = $"Supplier '{model.SupplierName}' created successfully.";
                _logger.LogInformation("Supplier created: {SupplierName}", model.SupplierName);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier");
                ModelState.AddModelError("", "Failed to create supplier. Please try again.");
                return View(model);
            }
        }

        // GET: Suppliers/Edit/5
        [AdminOnly]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(id);
                if (supplier == null)
                {
                    TempData["Error"] = "Supplier not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new SupplierEditViewModel
                {
                    SupplierID = supplier.SupplierID,
                    SupplierName = supplier.SupplierName,
                    ContactPerson = supplier.ContactPerson,
                    PhoneNumber = supplier.PhoneNumber,
                    Email = supplier.Email,
                    Address = supplier.Address,
                    IsActive = supplier.IsActive,
                    CreatedDate = supplier.CreatedDate
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading supplier for editing {SupplierId}", id);
                TempData["Error"] = "Failed to load supplier. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Edit(Guid id, SupplierEditViewModel model)
        {
            if (id != model.SupplierID)
            {
                TempData["Error"] = "Invalid supplier ID.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Validate supplier name uniqueness (excluding current supplier)
                var isNameUnique = await _supplierService.IsSupplierNameUniqueAsync(model.SupplierName, model.SupplierID);
                if (!isNameUnique)
                {
                    ModelState.AddModelError("SupplierName", "A supplier with this name already exists.");
                    return View(model);
                }

                var existingSupplier = await _supplierService.GetSupplierByIdAsync(id);
                if (existingSupplier == null)
                {
                    TempData["Error"] = "Supplier not found.";
                    return RedirectToAction(nameof(Index));
                }

                existingSupplier.SupplierName = model.SupplierName;
                existingSupplier.ContactPerson = model.ContactPerson ?? string.Empty;
                existingSupplier.PhoneNumber = model.PhoneNumber;
                existingSupplier.Email = model.Email;
                existingSupplier.Address = model.Address ?? string.Empty;
                existingSupplier.IsActive = model.IsActive;
                existingSupplier.UpdatedDate = DateTime.UtcNow;

                await _supplierService.UpdateSupplierAsync(existingSupplier);
                
                TempData["Success"] = $"Supplier '{model.SupplierName}' updated successfully.";
                _logger.LogInformation("Supplier updated: {SupplierId} ({SupplierName})", id, model.SupplierName);
                
                return RedirectToAction(nameof(Details), new { id = model.SupplierID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier {SupplierId}", id);
                ModelState.AddModelError("", "Failed to update supplier. Please try again.");
                return View(model);
            }
        }

        // GET: Suppliers/Delete/5
        [AdminOnly]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(id);
                if (supplier == null)
                {
                    TempData["Error"] = "Supplier not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new SupplierDeleteViewModel
                {
                    Supplier = supplier
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading supplier for deletion {SupplierId}", id);
                TempData["Error"] = "Failed to load supplier. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(id);
                if (supplier == null)
                {
                    TempData["Error"] = "Supplier not found.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _supplierService.DeleteSupplierAsync(id);
                if (result)
                {
                    TempData["Success"] = $"Supplier '{supplier.SupplierName}' deleted successfully.";
                    _logger.LogInformation("Supplier deleted: {SupplierId} ({SupplierName})", id, supplier.SupplierName);
                }
                else
                {
                    TempData["Error"] = "Failed to delete supplier.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier {SupplierId}", id);
                TempData["Error"] = "Failed to delete supplier. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Suppliers/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            try
            {
                var result = await _supplierService.ToggleSupplierStatusAsync(id);
                if (result)
                {
                    TempData["Success"] = "Supplier status updated successfully.";
                    _logger.LogInformation("Supplier status toggled: {SupplierId}", id);
                }
                else
                {
                    TempData["Error"] = "Failed to update supplier status.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling supplier status {SupplierId}", id);
                TempData["Error"] = "Failed to update supplier status. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Suppliers/Search
        public async Task<IActionResult> Search(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return RedirectToAction(nameof(Index));
                }

                var suppliers = await _supplierService.SearchSuppliersByNameAsync(query);
                var supplierList = suppliers.ToList();

                var viewModel = new SupplierListViewModel
                {
                    Suppliers = supplierList,
                    TotalSuppliers = supplierList.Count,
                    ActiveSuppliers = supplierList.Count(s => s.IsActive),
                    InactiveSuppliers = supplierList.Count(s => !s.IsActive),
                    SearchQuery = query
                };

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching suppliers with query: {Query}", query);
                TempData["Error"] = "Failed to search suppliers. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Suppliers/Active
        public async Task<IActionResult> Active()
        {
            try
            {
                var suppliers = await _supplierService.GetActiveSuppliersAsync();
                var supplierList = suppliers.ToList();

                var viewModel = new SupplierListViewModel
                {
                    Suppliers = supplierList,
                    TotalSuppliers = supplierList.Count,
                    ActiveSuppliers = supplierList.Count,
                    InactiveSuppliers = 0,
                    StatusFilter = "active"
                };

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading active suppliers");
                TempData["Error"] = "Failed to load active suppliers. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
