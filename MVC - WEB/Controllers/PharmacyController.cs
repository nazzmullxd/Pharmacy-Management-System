using Microsoft.AspNetCore.Mvc;
using MVC_WEB.Filters;
using MVC_WEB.Models.ViewModels;

namespace MVC_WEB.Controllers
{
    [Authenticated]
    public class PharmacyController : Controller
    {
        private readonly ILogger<PharmacyController> _logger;

        public PharmacyController(ILogger<PharmacyController> logger)
        {
            _logger = logger;
        }

        // GET: Pharmacy/Info
        public IActionResult Info()
        {
            // In a real application, load from database
            var viewModel = new PharmacyInfoViewModel
            {
                PharmacyName = "Pharmacy Management System",
                LicenseNumber = "PH-2024-001",
                OwnerName = "John Doe",
                PhoneNumber = "(555) 123-4567",
                Email = "info@pharmacy.com",
                Address = "123 Main Street",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                LicenseExpiry = DateTime.Today.AddYears(1),
                IsActive = true
            };

            return View(viewModel);
        }

        // GET: Pharmacy/Settings (Admin only)
        [AdminOnly]
        public IActionResult Settings()
        {
            // In a real application, load from database
            var viewModel = new PharmacySettingsViewModel
            {
                PharmacyName = "Pharmacy Management System",
                LicenseNumber = "PH-2024-001",
                OwnerName = "John Doe",
                PhoneNumber = "(555) 123-4567",
                Email = "info@pharmacy.com",
                Address = "123 Main Street",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                LicenseExpiry = DateTime.Today.AddYears(1),
                TaxRate = 8.5m,
                LowStockThreshold = 10,
                ExpiryAlertDays = 30,
                RequirePrescriptionForAntibiotics = true,
                InvoicePrefix = "INV",
                ReceiptFooter = "Thank you for your purchase!"
            };

            return View(viewModel);
        }

        // POST: Pharmacy/Settings (Admin only)
        [HttpPost]
        [AdminOnly]
        [ValidateAntiForgeryToken]
        public IActionResult Settings(PharmacySettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // In a real application, save to database
                TempData["Success"] = "Pharmacy settings saved successfully.";
                return RedirectToAction(nameof(Settings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving pharmacy settings");
                TempData["Error"] = "Failed to save settings.";
                return View(model);
            }
        }
    }
}
