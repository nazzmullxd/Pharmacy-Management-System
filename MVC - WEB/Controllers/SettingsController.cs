using Microsoft.AspNetCore.Mvc;
using MVC_WEB.Filters;
using MVC_WEB.Models.ViewModels;

namespace MVC_WEB.Controllers
{
    [Authenticated]
    public class SettingsController : Controller
    {
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(ILogger<SettingsController> logger)
        {
            _logger = logger;
        }

        // GET: Settings
        public IActionResult Index()
        {
            // Settings would typically be loaded from database or configuration
            var viewModel = new SettingsViewModel
            {
                General = new GeneralSettings
                {
                    Language = "en",
                    Timezone = "UTC",
                    DateFormat = "MM/dd/yyyy",
                    Currency = "USD"
                },
                Notifications = new NotificationSettings
                {
                    EmailNotifications = true,
                    LowStockAlerts = true,
                    ExpiryAlerts = true,
                    SaleNotifications = false
                },
                Display = new DisplaySettings
                {
                    Theme = "light",
                    ItemsPerPage = 10,
                    ShowDashboardStats = true
                }
            };

            return View(viewModel);
        }

        // POST: Settings/SaveGeneral
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveGeneral(GeneralSettings model)
        {
            try
            {
                // In a real application, save to database
                TempData["Success"] = "General settings saved successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving general settings");
                TempData["Error"] = "Failed to save settings.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Settings/SaveNotifications
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveNotifications(NotificationSettings model)
        {
            try
            {
                // In a real application, save to database
                TempData["Success"] = "Notification settings saved successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving notification settings");
                TempData["Error"] = "Failed to save settings.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Settings/SaveDisplay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveDisplay(DisplaySettings model)
        {
            try
            {
                // In a real application, save to database
                TempData["Success"] = "Display settings saved successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving display settings");
                TempData["Error"] = "Failed to save settings.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
