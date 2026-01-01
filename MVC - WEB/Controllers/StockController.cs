using Business.DTO;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MVC_WEB.Filters;
using MVC_WEB.Models.ViewModels;

namespace MVC_WEB.Controllers
{
    [Authenticated]
    public class StockController : Controller
    {
        private readonly IProductService _productService;
        private readonly IStockService _stockService;
        private readonly IStockAdjustmentService _stockAdjustmentService;
        private readonly ISupplierService _supplierService;
        private readonly ILogger<StockController> _logger;

        public StockController(
            IProductService productService,
            IStockService stockService,
            IStockAdjustmentService stockAdjustmentService,
            ISupplierService supplierService,
            ILogger<StockController> logger)
        {
            _productService = productService;
            _stockService = stockService;
            _stockAdjustmentService = stockAdjustmentService;
            _supplierService = supplierService;
            _logger = logger;
        }

        // GET: /Stock or /Stock/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                var stockItems = new List<ProductDTO>();

                foreach (var product in products)
                {
                    product.TotalStock = await _stockService.GetTotalStockForProductAsync(product.ProductID);
                    stockItems.Add(product);
                }

                var lowStockProducts = await _productService.GetLowStockProductsAsync();
                var lowStockAlerts = new List<ProductDTO>();
                foreach (var product in lowStockProducts)
                {
                    product.TotalStock = await _stockService.GetTotalStockForProductAsync(product.ProductID);
                    lowStockAlerts.Add(product);
                }

                var expiringAlerts = await _stockService.GetExpiryAlertsAsync();

                var viewModel = new StockIndexViewModel
                {
                    StockItems = stockItems,
                    LowStockAlerts = lowStockAlerts,
                    ExpiringAlerts = expiringAlerts,
                    TotalProducts = stockItems.Count,
                    LowStockItems = lowStockAlerts.Count,
                    ExpiringItems = expiringAlerts.Count(),
                    TotalStockValue = stockItems.Sum(p => p.TotalStock * p.UnitPrice)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stock data");
                return View(new StockIndexViewModel());
            }
        }

        // GET: /Stock/Items - Redirects to Index
        public IActionResult Items()
        {
            return RedirectToAction(nameof(Index));
        }

        // GET: /Stock/Batches
        public async Task<IActionResult> Batches(Guid? productId)
        {
            var batches = await _stockService.GetAllProductBatchesAsync();
            
            if (productId.HasValue)
            {
                batches = batches.Where(b => b.ProductID == productId.Value);
            }

            var viewModel = new StockBatchesViewModel
            {
                Batches = batches,
                ProductIdFilter = productId,
                TotalBatches = batches.Count(),
                ExpiringSoon = batches.Count(b => (b.ExpiryDate - DateTime.Now).TotalDays <= 30 && !b.IsExpired),
                LowStockBatches = batches.Count(b => b.QuantityInStock <= 10),
                TotalUnits = batches.Sum(b => b.QuantityInStock)
            };

            return View(viewModel);
        }

        // GET: /Stock/Expiring
        public async Task<IActionResult> Expiring(int days = 30)
        {
            var batches = await _stockService.GetExpiringBatchesAsync(days);

            var viewModel = new StockExpiringViewModel
            {
                Batches = batches,
                DaysAhead = days,
                ExpiredItems = batches.Count(b => b.ExpiryDate < DateTime.Now),
                ExpiringThisWeek = batches.Count(b => b.ExpiryDate >= DateTime.Now && (b.ExpiryDate - DateTime.Now).TotalDays <= 7),
                ExpiringThisMonth = batches.Count(b => (b.ExpiryDate - DateTime.Now).TotalDays <= 30 && (b.ExpiryDate - DateTime.Now).TotalDays > 7),
                AtRiskValue = batches.Sum(b => b.QuantityInStock * 10) // Estimated value
            };

            return View(viewModel);
        }

        // GET: /Stock/Adjustments (Admin Only)
        [AdminOnly]
        public async Task<IActionResult> Adjustments(Guid? productId)
        {
            var batches = await _stockService.GetAllProductBatchesAsync();
            var adjustments = await _stockAdjustmentService.GetAllStockAdjustmentsAsync();

            var viewModel = new StockAdjustmentsViewModel
            {
                Batches = batches,
                RecentAdjustments = adjustments.OrderByDescending(a => a.AdjustmentDate).Take(20),
                SelectedBatchId = null,
                ProductIdFilter = productId,
                Message = TempData["Message"] as string
            };

            return View(viewModel);
        }

        // POST: /Stock/Adjustments (Admin Only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Adjustments(StockAdjustmentCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var batches = await _stockService.GetAllProductBatchesAsync();
                var adjustments = await _stockAdjustmentService.GetAllStockAdjustmentsAsync();

                var viewModel = new StockAdjustmentsViewModel
                {
                    Batches = batches,
                    RecentAdjustments = adjustments.OrderByDescending(a => a.AdjustmentDate).Take(20),
                    SelectedBatchId = model.SelectedBatchId,
                    AdjustmentType = model.AdjustmentType,
                    AdjustedQuantity = model.AdjustedQuantity,
                    Reason = model.Reason
                };
                return View(viewModel);
            }

            var userIdStr = HttpContext.Session.GetString("userId");
            Guid.TryParse(userIdStr, out var userId);

            var adjustment = new StockAdjustmentDTO
            {
                StockAdjustmentID = Guid.NewGuid(),
                ProductBatchID = model.SelectedBatchId,
                AdjustmentType = model.AdjustmentType,
                AdjustedQuantity = model.AdjustedQuantity,
                Reason = model.Reason,
                UserID = userId,
                AdjustmentDate = DateTime.UtcNow
            };

            var result = await _stockAdjustmentService.CreateStockAdjustmentAsync(adjustment);

            bool stockUpdated = false;
            if (result.Success)
            {
                switch (model.AdjustmentType.ToLower())
                {
                    case "correction":
                        stockUpdated = await _stockService.ProcessStockAdjustmentAsync(model.SelectedBatchId, model.AdjustedQuantity, model.Reason);
                        break;
                    case "increase":
                        stockUpdated = await _stockService.AdjustStockAsync(model.SelectedBatchId, model.AdjustedQuantity, model.Reason);
                        break;
                    case "decrease":
                        stockUpdated = await _stockService.AdjustStockAsync(model.SelectedBatchId, -model.AdjustedQuantity, model.Reason);
                        break;
                }
            }

            TempData["Message"] = (result.Success && stockUpdated)
                ? "Stock adjusted successfully."
                : $"Failed to adjust stock: {result.ErrorMessage}";

            return RedirectToAction(nameof(Adjustments));
        }

        // GET: /Stock/AddBatch (Admin Only)
        [AdminOnly]
        public async Task<IActionResult> AddBatch()
        {
            var products = await _productService.GetAllProductsAsync();
            var suppliers = await _supplierService.GetAllSuppliersAsync();

            var viewModel = new StockAddBatchViewModel
            {
                Products = products,
                Suppliers = suppliers.Where(s => s.IsActive)
            };

            return View(viewModel);
        }

        // POST: /Stock/AddBatch (Admin Only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> AddBatch(StockAddBatchViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Products = await _productService.GetAllProductsAsync();
                model.Suppliers = (await _supplierService.GetAllSuppliersAsync()).Where(s => s.IsActive);
                return View(model);
            }

            var batch = new ProductBatchDTO
            {
                ProductBatchID = Guid.NewGuid(),
                ProductID = model.ProductID,
                SupplierID = model.SupplierID,
                BatchNumber = model.BatchNumber,
                ExpiryDate = model.ExpiryDate,
                QuantityInStock = model.QuantityInStock,
                CreatedDate = DateTime.UtcNow
            };

            await _stockService.CreateProductBatchAsync(batch);
            TempData["Message"] = "Batch added successfully.";
            return RedirectToAction(nameof(Batches));
        }

        // GET: /Stock/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var batches = await _stockService.GetBatchesByProductAsync(id);
            product.TotalStock = await _stockService.GetTotalStockForProductAsync(id);

            var viewModel = new StockDetailsViewModel
            {
                Product = product,
                Batches = batches
            };

            return View(viewModel);
        }

        // GET: /Stock/LowStock
        public async Task<IActionResult> LowStock()
        {
            var lowStockProducts = await _productService.GetLowStockProductsAsync();
            var productList = new List<ProductDTO>();
            
            foreach (var product in lowStockProducts)
            {
                product.TotalStock = await _stockService.GetTotalStockForProductAsync(product.ProductID);
                productList.Add(product);
            }

            var viewModel = new StockLowStockViewModel
            {
                LowStockProducts = productList
            };

            return View(viewModel);
        }

        // POST: /Stock/DeleteBatch/{id} (Admin Only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> DeleteBatch(Guid id)
        {
            var result = await _stockService.DeleteProductBatchAsync(id);
            TempData["Message"] = result ? "Batch deleted successfully." : "Failed to delete batch.";
            return RedirectToAction(nameof(Batches));
        }
    }
}
