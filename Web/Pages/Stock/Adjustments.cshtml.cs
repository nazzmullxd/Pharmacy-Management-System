using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Stock
{
    public class AdjustmentsModel : PageModel
    {
        private readonly IStockService _stockService;
        private readonly IStockAdjustmentService _stockAdjustmentService;

        public AdjustmentsModel(IStockService stockService, IStockAdjustmentService stockAdjustmentService)
        {
            _stockService = stockService;
            _stockAdjustmentService = stockAdjustmentService;
        }

        [BindProperty]
        public Guid SelectedBatchId { get; set; }

        [BindProperty]
        public string AdjustmentType { get; set; } = "Correction"; // Increase, Decrease, Correction

        [BindProperty]
        public int AdjustedQuantity { get; set; }

        [BindProperty]
        public string Reason { get; set; } = string.Empty;

        public IEnumerable<ProductBatchDTO> Batches { get; set; } = Enumerable.Empty<ProductBatchDTO>();

        public string? Message { get; set; }

        public async Task OnGet()
        {
            Batches = await _stockService.GetAllProductBatchesAsync();
            Message = TempData["Message"] as string;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (SelectedBatchId == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "Please select a batch.");
            }
            if (!ModelState.IsValid)
            {
                Batches = await _stockService.GetAllProductBatchesAsync();
                return Page();
            }

            var adjustment = new StockAdjustmentDTO
            {
                StockAdjustmentID = Guid.NewGuid(),
                ProductBatchID = SelectedBatchId,
                AdjustmentType = AdjustmentType,
                AdjustedQuantity = AdjustedQuantity,
                Reason = Reason,
                AdjustmentDate = DateTime.UtcNow
            };

            var ok = await _stockAdjustmentService.ProcessStockAdjustmentAsync(adjustment);
            TempData["Message"] = ok ? "Stock adjusted successfully." : "Failed to adjust stock.";
            return RedirectToPage();
        }
    }
}


