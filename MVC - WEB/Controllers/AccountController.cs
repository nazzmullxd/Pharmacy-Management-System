using Business.DTO;
using Business.Interfaces;
using Business.Services;
using Microsoft.AspNetCore.Mvc;
using MVC_WEB.Models.ViewModels.Account;

namespace MVC_WEB.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserService userService, ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        #region Login

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // If already authenticated, redirect to dashboard
            if (HttpContext.Session.GetString("auth") == "1")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Get user by email
                var user = await _userService.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(model);
                }

                // Authenticate user
                var isValid = await _userService.AuthenticateUserAsync(model.Email, model.Password);
                if (!isValid)
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(model);
                }

                // Update last login
                await _userService.UpdateLastLoginAsync(user.UserID);

                // Set session values
                HttpContext.Session.SetString("auth", "1");
                HttpContext.Session.SetString("userId", user.UserID.ToString());
                HttpContext.Session.SetString("userEmail", user.Email);
                HttpContext.Session.SetString("userName", user.FullName);
                HttpContext.Session.SetString("role", user.Role);

                _logger.LogInformation("User {Email} logged in successfully with role {Role}", user.Email, user.Role);

                // Redirect to return URL or dashboard
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        #endregion

        #region Register

        [HttpGet]
        public IActionResult Register()
        {
            // If already authenticated, redirect to dashboard
            if (HttpContext.Session.GetString("auth") == "1")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Check if email already exists
                var isEmailUnique = await _userService.IsEmailUniqueAsync(model.Email);
                if (!isEmailUnique)
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(model);
                }

                // Create user DTO
                var userDto = new UserDTO
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Role = model.Role
                };

                // Create user
                var createdUser = await _userService.CreateUserAsync(userDto, model.Password);

                _logger.LogInformation("New user registered: {Email} with role {Role}", model.Email, model.Role);

                TempData["SuccessMessage"] = "Registration successful! Please login with your credentials.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        #endregion

        #region Logout

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            var userEmail = HttpContext.Session.GetString("userEmail");
            
            // Clear session
            HttpContext.Session.Clear();

            _logger.LogInformation("User {Email} logged out", userEmail);

            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult LogoutConfirm()
        {
            return View();
        }

        #endregion

        #region Access Denied

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #endregion
    }
}
