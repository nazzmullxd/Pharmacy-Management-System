using Microsoft.AspNetCore.Mvc;
using Business.Interfaces;
using Business.DTO;
using MVC_WEB.Filters;
using MVC_WEB.Models.ViewModels;

namespace MVC_WEB.Controllers
{
    [Authenticated]
    public class SupportController : Controller
    {
        private readonly ISupportTicketService _supportTicketService;
        private readonly ILogger<SupportController> _logger;

        public SupportController(
            ISupportTicketService supportTicketService,
            ILogger<SupportController> logger)
        {
            _supportTicketService = supportTicketService;
            _logger = logger;
        }

        // GET: Support
        public async Task<IActionResult> Index(string? status)
        {
            try
            {
                var tickets = await _supportTicketService.GetAllTicketsAsync();
                var ticketList = tickets.ToList();

                // Filter by user role - Users only see their own tickets
                var userRole = HttpContext.Session.GetString("role") ?? "User";
                var userIdStr = HttpContext.Session.GetString("userId");
                
                if (userRole != "Admin" && Guid.TryParse(userIdStr, out Guid userId))
                {
                    ticketList = ticketList.Where(t => t.CreatedBy == userId).ToList();
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(status))
                {
                    ticketList = ticketList.Where(t => t.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                var viewModel = new SupportIndexViewModel
                {
                    Tickets = ticketList,
                    TotalTickets = ticketList.Count,
                    OpenTickets = ticketList.Count(t => t.Status != "Closed" && t.Status != "Resolved"),
                    ClosedTickets = ticketList.Count(t => t.Status == "Closed" || t.Status == "Resolved"),
                    StatusFilter = status
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading support tickets");
                TempData["Error"] = "Failed to load support tickets.";
                return View(new SupportIndexViewModel());
            }
        }

        // GET: Support/Create
        public IActionResult Create()
        {
            return View(new SupportCreateViewModel());
        }

        // POST: Support/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupportCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userIdStr = HttpContext.Session.GetString("userId");
                var userName = HttpContext.Session.GetString("userName") ?? "Unknown";
                
                if (!Guid.TryParse(userIdStr, out Guid userId))
                {
                    TempData["Error"] = "Session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                var ticket = new SupportTicketDTO
                {
                    Title = model.Title,
                    Description = model.Description,
                    Category = model.Category,
                    Priority = model.Priority,
                    Status = "Open",
                    CreatedBy = userId,
                    CreatedByName = userName,
                    CreatedDate = DateTime.Now
                };

                await _supportTicketService.CreateTicketAsync(ticket);
                TempData["Success"] = "Support ticket created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating support ticket");
                TempData["Error"] = "Failed to create support ticket.";
                return View(model);
            }
        }

        // GET: Support/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var ticket = await _supportTicketService.GetTicketByIdAsync(id);
                if (ticket == null)
                {
                    TempData["Error"] = "Ticket not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check access rights - Users can only see their own tickets
                var userRole = HttpContext.Session.GetString("role") ?? "User";
                var userIdStr = HttpContext.Session.GetString("userId");
                
                if (userRole != "Admin" && Guid.TryParse(userIdStr, out Guid userId) && ticket.CreatedBy != userId)
                {
                    TempData["Error"] = "Access denied.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new SupportDetailsViewModel
                {
                    Ticket = ticket
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ticket details for ID: {TicketId}", id);
                TempData["Error"] = "Failed to load ticket details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Support/UpdateStatus (Admin only)
        [HttpPost]
        [AdminOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, string status, string? resolution)
        {
            try
            {
                var ticket = await _supportTicketService.GetTicketByIdAsync(id);
                if (ticket == null)
                {
                    TempData["Error"] = "Ticket not found.";
                    return RedirectToAction(nameof(Index));
                }

                ticket.Status = status;
                ticket.Resolution = resolution ?? string.Empty;

                await _supportTicketService.UpdateTicketAsync(ticket);
                TempData["Success"] = "Ticket status updated successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket status for ID: {TicketId}", id);
                TempData["Error"] = "Failed to update ticket status.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
