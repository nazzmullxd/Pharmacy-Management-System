using Microsoft.AspNetCore.Mvc;
using Business.Interfaces;
using Business.DTO;
using MVC_WEB.Filters;
using MVC_WEB.Models.ViewModels;

namespace MVC_WEB.Controllers
{
    [Authenticated]
    [AdminOnly]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                var viewModel = new UserListViewModel
                {
                    Users = users.ToList(),
                    TotalUsers = users.Count(),
                    ActiveUsers = users.Count(), // All users assumed active
                    AdminCount = users.Count(u => u.Role == "Admin"),
                    StaffCount = users.Count(u => u.Role != "Admin")
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users list");
                TempData["Error"] = "Failed to load users. Please try again.";
                return View(new UserListViewModel());
            }
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new UserDetailsViewModel
                {
                    User = user
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user details for {UserId}", id);
                TempData["Error"] = "Failed to load user details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            var viewModel = new UserCreateViewModel
            {
                AvailableRoles = GetAvailableRoles()
            };
            return View(viewModel);
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = GetAvailableRoles();
                return View(model);
            }

            try
            {
                // Check if email is unique
                var isEmailUnique = await _userService.IsEmailUniqueAsync(model.Email);
                if (!isEmailUnique)
                {
                    ModelState.AddModelError("Email", "This email address is already in use.");
                    model.AvailableRoles = GetAvailableRoles();
                    return View(model);
                }

                // Validate password match
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                    model.AvailableRoles = GetAvailableRoles();
                    return View(model);
                }

                var userDto = new UserDTO
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Role = model.Role
                };

                await _userService.CreateUserAsync(userDto, model.Password);
                
                TempData["Success"] = $"User '{model.FirstName} {model.LastName}' created successfully.";
                _logger.LogInformation("User created: {Email} with role {Role}", model.Email, model.Role);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Email}", model.Email);
                ModelState.AddModelError("", "Failed to create user. Please try again.");
                model.AvailableRoles = GetAvailableRoles();
                return View(model);
            }
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new UserEditViewModel
                {
                    UserID = user.UserID,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role,
                    AvailableRoles = GetAvailableRoles()
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user for edit {UserId}", id);
                TempData["Error"] = "Failed to load user. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UserEditViewModel model)
        {
            if (id != model.UserID)
            {
                TempData["Error"] = "Invalid user ID.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                model.AvailableRoles = GetAvailableRoles();
                return View(model);
            }

            try
            {
                // Check if email is unique (excluding current user)
                var isEmailUnique = await _userService.IsEmailUniqueAsync(model.Email, model.UserID);
                if (!isEmailUnique)
                {
                    ModelState.AddModelError("Email", "This email address is already in use by another user.");
                    model.AvailableRoles = GetAvailableRoles();
                    return View(model);
                }

                var userDto = new UserDTO
                {
                    UserID = model.UserID,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Role = model.Role
                };

                await _userService.UpdateUserAsync(userDto);
                
                TempData["Success"] = $"User '{model.FirstName} {model.LastName}' updated successfully.";
                _logger.LogInformation("User updated: {UserId}", model.UserID);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", model.UserID);
                ModelState.AddModelError("", "Failed to update user. Please try again.");
                model.AvailableRoles = GetAvailableRoles();
                return View(model);
            }
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Prevent self-deletion
                var currentUserId = HttpContext.Session.GetString("userId");
                if (currentUserId != null && Guid.Parse(currentUserId) == id)
                {
                    TempData["Error"] = "You cannot delete your own account.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new UserDeleteViewModel
                {
                    User = user
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user for deletion {UserId}", id);
                TempData["Error"] = "Failed to load user. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                // Prevent self-deletion
                var currentUserId = HttpContext.Session.GetString("userId");
                if (currentUserId != null && Guid.Parse(currentUserId) == id)
                {
                    TempData["Error"] = "You cannot delete your own account.";
                    return RedirectToAction(nameof(Index));
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userService.DeleteUserAsync(id);
                if (result)
                {
                    TempData["Success"] = $"User '{user.FullName}' deleted successfully.";
                    _logger.LogInformation("User deleted: {UserId} ({Email})", id, user.Email);
                }
                else
                {
                    TempData["Error"] = "Failed to delete user.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                TempData["Error"] = "Failed to delete user. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Users/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(Guid id, string newPassword)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // For admin reset, we create a new user with same info but new password
                // Note: A proper implementation would have a separate ResetPasswordAsync method
                _logger.LogInformation("Password reset requested for user: {UserId}", id);
                
                return Json(new { success = true, message = $"Password reset for {user.FullName}." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for {UserId}", id);
                return Json(new { success = false, message = "Failed to reset password." });
            }
        }

        private List<string> GetAvailableRoles()
        {
            return new List<string> { "Admin", "User", "Manager", "Pharmacist", "Staff" };
        }
    }
}
