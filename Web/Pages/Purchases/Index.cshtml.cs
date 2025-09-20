using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Purchases
{
    public class IndexModel : PageModel
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public IndexModel(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        public IEnumerable<PurchaseOrderDTO> Orders { get; set; } = Enumerable.Empty<PurchaseOrderDTO>();

        public async Task OnGet()
        {
            Orders = await _purchaseOrderService.GetAllPurchaseOrdersAsync() ?? Enumerable.Empty<PurchaseOrderDTO>();
        }

        public async Task<IActionResult> OnPostProcessOrderAsync(Guid orderId)
        {
            try
            {
                var result = await _purchaseOrderService.ProcessPurchaseOrderAsync(orderId, Guid.NewGuid()); // TODO: Use actual user ID
                
                if (result)
                {
                    TempData["Message"] = "Purchase order has been processed successfully and inventory has been updated.";
                }
                else
                {
                    TempData["Error"] = "Failed to process the purchase order. Please ensure it's in pending status.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while processing the order: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApproveOrderAsync(Guid orderId)
        {
            try
            {
                var result = await _purchaseOrderService.ApprovePurchaseOrderAsync(orderId, Guid.NewGuid()); // TODO: Use actual user ID
                
                if (result)
                {
                    TempData["Message"] = "Purchase order has been approved successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to approve the purchase order.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while approving the order: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelOrderAsync(Guid orderId, string reason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    TempData["Error"] = "Please provide a reason for cancellation.";
                    return RedirectToPage();
                }

                var result = await _purchaseOrderService.CancelPurchaseOrderAsync(orderId, reason);
                
                if (result)
                {
                    TempData["Message"] = "Purchase order has been cancelled successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to cancel the purchase order.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while cancelling the order: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
