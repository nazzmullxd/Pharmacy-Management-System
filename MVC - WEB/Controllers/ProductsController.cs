using Microsoft.AspNetCore.Mvc;
using Business.Interfaces;
using Business.DTO;
using MVC_WEB.Filters;
using MVC_WEB.Models.ViewModels;

namespace MVC_WEB.Controllers
{
    [Authenticated]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                var productList = products.ToList();
                
                var viewModel = new ProductListViewModel
                {
                    Products = productList,
                    TotalProducts = productList.Count,
                    InStockCount = productList.Count(p => p.TotalStock > 0),
                    LowStockCount = productList.Count(p => p.IsLowStock),
                    OutOfStockCount = productList.Count(p => p.TotalStock == 0)
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products list");
                TempData["Error"] = "Failed to load products. Please try again.";
                return View(new ProductListViewModel());
            }
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new ProductDetailsViewModel
                {
                    Product = product
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product details for {ProductId}", id);
                TempData["Error"] = "Failed to load product details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Products/Create
        [AdminOnly]
        public IActionResult Create()
        {
            var viewModel = new ProductCreateViewModel
            {
                IsActive = true,
                LowStockThreshold = 10,
                Categories = GetProductCategories()
            };
            return View(viewModel);
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = GetProductCategories();
                return View(model);
            }

            try
            {
                // Validate business rules
                if (model.UnitPrice <= 0)
                {
                    ModelState.AddModelError("UnitPrice", "Unit price must be greater than 0.");
                    model.Categories = GetProductCategories();
                    return View(model);
                }

                if (model.DefaultRetailPrice <= model.UnitPrice)
                {
                    ModelState.AddModelError("DefaultRetailPrice", "Retail price must be greater than unit price.");
                    model.Categories = GetProductCategories();
                    return View(model);
                }

                // Check if barcode is unique (if provided)
                if (!string.IsNullOrEmpty(model.Barcode))
                {
                    var isUnique = await _productService.IsBarcodeUniqueAsync(model.Barcode);
                    if (!isUnique)
                    {
                        ModelState.AddModelError("Barcode", "This barcode is already in use.");
                        model.Categories = GetProductCategories();
                        return View(model);
                    }
                }

                var productDto = new ProductDTO
                {
                    ProductID = Guid.NewGuid(),
                    ProductName = model.ProductName,
                    GenericName = model.GenericName,
                    Manufacturer = model.Manufacturer,
                    Category = model.Category,
                    Description = model.Description ?? string.Empty,
                    UnitPrice = model.UnitPrice,
                    DefaultRetailPrice = model.DefaultRetailPrice,
                    DefaultWholeSalePrice = model.DefaultWholeSalePrice,
                    IsActive = model.IsActive,
                    Barcode = model.Barcode ?? string.Empty,
                    LowStockThreshold = model.LowStockThreshold,
                    CreatedDate = DateTime.UtcNow,
                    TotalStock = 0
                };

                await _productService.CreateProductAsync(productDto);
                
                TempData["Success"] = $"Product '{model.ProductName}' created successfully.";
                _logger.LogInformation("Product created: {ProductName}", model.ProductName);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product {ProductName}", model.ProductName);
                ModelState.AddModelError("", "Failed to create product. Please try again.");
                model.Categories = GetProductCategories();
                return View(model);
            }
        }

        // GET: Products/Edit/5
        [AdminOnly]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new ProductEditViewModel
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    GenericName = product.GenericName,
                    Manufacturer = product.Manufacturer,
                    Category = product.Category,
                    Description = product.Description,
                    UnitPrice = product.UnitPrice,
                    DefaultRetailPrice = product.DefaultRetailPrice,
                    DefaultWholeSalePrice = product.DefaultWholeSalePrice,
                    IsActive = product.IsActive,
                    Barcode = product.Barcode,
                    LowStockThreshold = product.LowStockThreshold,
                    TotalStock = product.TotalStock,
                    Categories = GetProductCategories()
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product for edit {ProductId}", id);
                TempData["Error"] = "Failed to load product. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Edit(Guid id, ProductEditViewModel model)
        {
            if (id != model.ProductID)
            {
                TempData["Error"] = "Invalid product ID.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                model.Categories = GetProductCategories();
                return View(model);
            }

            try
            {
                // Check if barcode is unique (excluding current product)
                if (!string.IsNullOrEmpty(model.Barcode))
                {
                    var isUnique = await _productService.IsBarcodeUniqueAsync(model.Barcode, model.ProductID);
                    if (!isUnique)
                    {
                        ModelState.AddModelError("Barcode", "This barcode is already in use by another product.");
                        model.Categories = GetProductCategories();
                        return View(model);
                    }
                }

                var productDto = new ProductDTO
                {
                    ProductID = model.ProductID,
                    ProductName = model.ProductName,
                    GenericName = model.GenericName,
                    Manufacturer = model.Manufacturer,
                    Category = model.Category,
                    Description = model.Description ?? string.Empty,
                    UnitPrice = model.UnitPrice,
                    DefaultRetailPrice = model.DefaultRetailPrice,
                    DefaultWholeSalePrice = model.DefaultWholeSalePrice,
                    IsActive = model.IsActive,
                    Barcode = model.Barcode ?? string.Empty,
                    LowStockThreshold = model.LowStockThreshold,
                    TotalStock = model.TotalStock
                };

                await _productService.UpdateProductAsync(productDto);
                
                TempData["Success"] = $"Product '{model.ProductName}' updated successfully.";
                _logger.LogInformation("Product updated: {ProductId}", model.ProductID);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", model.ProductID);
                ModelState.AddModelError("", "Failed to update product. Please try again.");
                model.Categories = GetProductCategories();
                return View(model);
            }
        }

        // GET: Products/Delete/5
        [AdminOnly]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new ProductDeleteViewModel
                {
                    Product = product
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product for deletion {ProductId}", id);
                TempData["Error"] = "Failed to load product. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _productService.DeleteProductAsync(id);
                if (result)
                {
                    TempData["Success"] = $"Product '{product.ProductName}' deleted successfully.";
                    _logger.LogInformation("Product deleted: {ProductId} ({ProductName})", id, product.ProductName);
                }
                else
                {
                    TempData["Error"] = "Failed to delete product.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                TempData["Error"] = "Failed to delete product. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Products/Categories
        public async Task<IActionResult> Categories()
        {
            var categories = GetProductCategories();
            var categoryCounts = new Dictionary<string, int>();
            
            // Get product counts for each category
            var allProducts = await _productService.GetAllProductsAsync();
            var productList = allProducts.ToList();
            
            foreach (var category in categories)
            {
                categoryCounts[category] = productList.Count(p => p.Category == category);
            }
            
            var viewModel = new ProductCategoriesViewModel
            {
                Categories = categories,
                CategoryCounts = categoryCounts,
                IsAdmin = HttpContext.Session.GetString("role") == "Admin"
            };
            return View(viewModel);
        }

        // POST: Products/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            try
            {
                var result = await _productService.ToggleProductStatusAsync(id);
                if (result)
                {
                    TempData["Success"] = "Product status updated successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to update product status.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling product status {ProductId}", id);
                TempData["Error"] = "Failed to update product status. Please try again.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Search
        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            try
            {
                var products = await _productService.SearchProductsByNameAsync(query ?? string.Empty);
                return Json(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with query {Query}", query);
                return Json(new List<ProductDTO>());
            }
        }

        // GET: Products/LowStock
        public async Task<IActionResult> LowStock()
        {
            try
            {
                var products = await _productService.GetLowStockProductsAsync();
                var viewModel = new ProductListViewModel
                {
                    Products = products.ToList(),
                    TotalProducts = products.Count(),
                    LowStockCount = products.Count()
                };
                ViewData["Title"] = "Low Stock Products";
                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading low stock products");
                TempData["Error"] = "Failed to load low stock products.";
                return RedirectToAction(nameof(Index));
            }
        }

        private List<string> GetProductCategories()
        {
            return new List<string>
            {
                "Pain Relief",
                "Antibiotic",
                "Supplements",
                "Digestive",
                "Diabetes",
                "Cardiovascular",
                "Respiratory",
                "Dermatology",
                "Vitamins",
                "Cold & Flu",
                "First Aid",
                "Other"
            };
        }
    }
}
