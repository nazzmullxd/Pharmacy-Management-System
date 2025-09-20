using Business.DTO;
using Business.Interfaces;
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
    }
}
