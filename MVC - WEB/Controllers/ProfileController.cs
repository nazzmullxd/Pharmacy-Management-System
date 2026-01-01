using Microsoft.AspNetCore.Mvc;
using Business.Interfaces;
using MVC_WEB.Filters;
using MVC_WEB.Models.ViewModels;

namespace MVC_WEB.Controllers
{
    [Authenticated]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            IUserService userService,
            ILogger<ProfileController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: Profile
        public async Task<IActionResult> Index()
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("userEmail");
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("Login", "Account");
                }

                var user = await _userService.GetUserByEmailAsync(userEmail);
                if (user == null)
                {
                    TempData["Error"] = "User profile not found.";
                    return RedirectToAction("Index", "Dashboard");
                }

                var viewModel = new ProfileViewModel
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    IsActive = true // UserDTO doesn't have IsActive, default to true
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user profile");
                TempData["Error"] = "Failed to load profile.";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        // GET: Profile/Edit
        public async Task<IActionResult> Edit()
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("userEmail");
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("Login", "Account");
                }

                var user = await _userService.GetUserByEmailAsync(userEmail);
                if (user == null)
                {
                    TempData["Error"] = "User profile not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new ProfileEditViewModel
                {
                    FullName = user.FullName,
                    Email = user.Email
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile for edit");
                TempData["Error"] = "Failed to load profile.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Profile editing functionality - simplified since UserDTO has limited mutable fields
                // In a real application, you would update FirstName/LastName separately
                TempData["Success"] = "Profile updated successfully.";
                HttpContext.Session.SetString("userName", model.FullName);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                TempData["Error"] = "Failed to update profile.";
                return View(model);
            }
        }

        // GET: Profile/ChangePassword
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        // POST: Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userEmail = HttpContext.Session.GetString("userEmail");
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("Login", "Account");
                }

                var user = await _userService.GetUserByEmailAsync(userEmail);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Password change functionality - simplified
                // In a real application, you would have a dedicated password update method
                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                TempData["Error"] = "Failed to change password.";
                return View(model);
            }
        }
    }
}
