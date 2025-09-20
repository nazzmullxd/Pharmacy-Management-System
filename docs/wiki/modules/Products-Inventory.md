# 📦 Products & Inventory Module

Comprehensive guide for managing products, inventory, stock levels, and batch tracking in the Pharmacy Management System.

## 🎯 Module Overview

The Products & Inventory module is the backbone of the pharmacy system, managing all product information, stock levels, batch tracking, and inventory operations.

### Key Features
- **Product Catalog Management**: Complete product information and categorization
- **Batch Tracking**: Detailed batch management with expiry date monitoring
- **Stock Level Management**: Real-time inventory tracking and alerts
- **Automatic Reordering**: Smart reorder point management
- **Expiry Date Monitoring**: Comprehensive expiry alert system
- **Barcode Support**: Barcode scanning and generation
- **Category Management**: Hierarchical product categorization
- **Supplier Integration**: Product-supplier relationship management

### Business Value
- **Inventory Accuracy**: Real-time stock tracking prevents stockouts
- **Cost Control**: Optimized inventory levels reduce carrying costs
- **Compliance**: FDA-compliant batch tracking and expiry management
- **Efficiency**: Automated processes reduce manual inventory tasks
- **Safety**: Expiry alerts prevent dispensing expired medications

## 🔧 Technical Architecture

### Data Models

#### Product Entity
```csharp
public class Product
{
    public Guid ProductID { get; set; }
    public string ProductName { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string Manufacturer { get; set; }
    public decimal UnitPrice { get; set; }
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
    public int MaxStockLevel { get; set; }
    public string Unit { get; set; }
    public string Barcode { get; set; }
    public bool IsControlledSubstance { get; set; }
    public bool RequiresPrescription { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdated { get; set; }
    
    // Navigation Properties
    public Guid? SupplierID { get; set; }
    public Supplier Supplier { get; set; }
    public List<ProductBatch> ProductBatches { get; set; }
    public List<SaleItem> SaleItems { get; set; }
    public List<PurchaseItem> PurchaseItems { get; set; }
    public List<StockAdjustment> StockAdjustments { get; set; }
}
```

#### Product Batch Entity
```csharp
public class ProductBatch
{
    public Guid BatchID { get; set; }
    public Guid ProductID { get; set; }
    public string BatchNumber { get; set; }
    public DateTime ManufactureDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int InitialQuantity { get; set; }
    public int CurrentStock { get; set; }
    public decimal CostPrice { get; set; }
    public string Location { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    
    // Navigation Properties
    public Product Product { get; set; }
    public List<BatchMovement> BatchMovements { get; set; }
}
```

#### Stock Adjustment Entity
```csharp
public class StockAdjustment
{
    public Guid AdjustmentID { get; set; }
    public Guid ProductID { get; set; }
    public Guid? BatchID { get; set; }
    public string AdjustmentType { get; set; } // Increase, Decrease, Correction
    public int Quantity { get; set; }
    public string Reason { get; set; }
    public string Notes { get; set; }
    public DateTime AdjustmentDate { get; set; }
    public Guid AdjustedBy { get; set; }
    
    // Navigation Properties
    public Product Product { get; set; }
    public ProductBatch ProductBatch { get; set; }
    public User User { get; set; }
}
```

### Service Layer Architecture

#### Product Service Interface
```csharp
public interface IProductService
{
    // Product Management
    Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
    Task<ProductDTO> GetProductByIdAsync(Guid productId);
    Task<ProductDTO> CreateProductAsync(ProductDTO productDto);
    Task<ProductDTO> UpdateProductAsync(ProductDTO productDto);
    Task<bool> DeleteProductAsync(Guid productId);
    
    // Search and Filtering
    Task<IEnumerable<ProductDTO>> SearchProductsAsync(string searchTerm);
    Task<IEnumerable<ProductDTO>> GetProductsByCategoryAsync(string category);
    Task<IEnumerable<ProductDTO>> GetLowStockProductsAsync();
    Task<ProductDTO> GetProductByBarcodeAsync(string barcode);
    
    // Stock Management
    Task<int> GetAvailableStockAsync(Guid productId);
    Task UpdateStockAsync(Guid productId, int quantity, string reason);
    Task<bool> CheckStockAvailabilityAsync(Guid productId, int requiredQuantity);
    
    // Batch Management
    Task<IEnumerable<ProductBatchDTO>> GetProductBatchesAsync(Guid productId);
    Task<ProductBatchDTO> CreateProductBatchAsync(ProductBatchDTO batchDto);
    Task<IEnumerable<ProductBatchDTO>> GetExpiringBatchesAsync(int daysAhead = 30);
    
    // Reports and Analytics
    Task<IEnumerable<ProductDTO>> GetTopSellingProductsAsync(int count = 10);
    Task<IEnumerable<ExpiryAlertDTO>> GetExpiryAlertsAsync();
    Task<InventoryReportDTO> GenerateInventoryReportAsync();
}
```

## 📋 Product Management

### Creating Products
```csharp
public async Task<ProductDTO> CreateProductAsync(ProductDTO productDto)
{
    try
    {
        // Validate product data
        await ValidateProductDataAsync(productDto);
        
        // Check for duplicate barcode
        if (!string.IsNullOrEmpty(productDto.Barcode))
        {
            var existingProduct = await GetProductByBarcodeAsync(productDto.Barcode);
            if (existingProduct != null)
            {
                throw new InvalidOperationException($"Product with barcode {productDto.Barcode} already exists");
            }
        }
        
        // Generate barcode if not provided
        if (string.IsNullOrEmpty(productDto.Barcode))
        {
            productDto.Barcode = await GenerateBarcodeAsync();
        }
        
        var product = new Product
        {
            ProductID = Guid.NewGuid(),
            ProductName = productDto.ProductName,
            Description = productDto.Description,
            Category = productDto.Category,
            Manufacturer = productDto.Manufacturer,
            UnitPrice = productDto.UnitPrice,
            CurrentStock = 0, // Initial stock is 0
            ReorderLevel = productDto.ReorderLevel,
            MaxStockLevel = productDto.MaxStockLevel,
            Unit = productDto.Unit,
            Barcode = productDto.Barcode,
            IsControlledSubstance = productDto.IsControlledSubstance,
            RequiresPrescription = productDto.RequiresPrescription,
            SupplierID = productDto.SupplierID,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Product created: {ProductName} with ID {ProductID}", 
            product.ProductName, product.ProductID);
        
        return MapToDTO(product);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating product: {ProductName}", productDto.ProductName);
        throw;
    }
}
```

### Product Categories
The system supports hierarchical product categorization:

#### Standard Categories
- **Prescription Medications**
  - Antibiotics
  - Cardiovascular
  - Diabetes
  - Mental Health
  - Pain Management
  - Respiratory
- **Over-the-Counter (OTC)**
  - Pain Relief
  - Cold & Flu
  - Allergies
  - Digestive Health
  - Vitamins & Supplements
- **Medical Supplies**
  - First Aid
  - Diagnostic Tools
  - Mobility Aids
  - Personal Care
- **Health & Beauty**
  - Skincare
  - Hair Care
  - Oral Care
  - Cosmetics

### Product Information Management
```csharp
public class ProductDTO
{
    public Guid ProductID { get; set; }
    public string ProductName { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string Manufacturer { get; set; }
    public decimal UnitPrice { get; set; }
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
    public int MaxStockLevel { get; set; }
    public string Unit { get; set; }
    public string Barcode { get; set; }
    public bool IsControlledSubstance { get; set; }
    public bool RequiresPrescription { get; set; }
    public Guid? SupplierID { get; set; }
    public string SupplierName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdated { get; set; }
    
    // Additional Properties
    public List<ProductBatchDTO> Batches { get; set; }
    public int AvailableStock { get; set; }
    public DateTime? NearestExpiryDate { get; set; }
    public bool IsLowStock => CurrentStock <= ReorderLevel;
    public bool IsOutOfStock => CurrentStock <= 0;
}
```

## 📦 Batch Management

### Batch Tracking System
The system implements comprehensive batch tracking for all products:

#### Batch Creation
```csharp
public async Task<ProductBatchDTO> CreateProductBatchAsync(ProductBatchDTO batchDto)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // Validate batch data
        await ValidateBatchDataAsync(batchDto);
        
        // Check for duplicate batch number
        var existingBatch = await _context.ProductBatches
            .FirstOrDefaultAsync(b => b.BatchNumber == batchDto.BatchNumber && 
                                    b.ProductID == batchDto.ProductID);
        
        if (existingBatch != null)
        {
            throw new InvalidOperationException($"Batch {batchDto.BatchNumber} already exists for this product");
        }
        
        var batch = new ProductBatch
        {
            BatchID = Guid.NewGuid(),
            ProductID = batchDto.ProductID,
            BatchNumber = batchDto.BatchNumber,
            ManufactureDate = batchDto.ManufactureDate,
            ExpiryDate = batchDto.ExpiryDate,
            InitialQuantity = batchDto.InitialQuantity,
            CurrentStock = batchDto.InitialQuantity,
            CostPrice = batchDto.CostPrice,
            Location = batchDto.Location,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        
        _context.ProductBatches.Add(batch);
        
        // Update product stock
        var product = await _context.Products.FindAsync(batchDto.ProductID);
        product.CurrentStock += batchDto.InitialQuantity;
        product.LastUpdated = DateTime.UtcNow;
        
        // Create stock movement record
        var movement = new BatchMovement
        {
            MovementID = Guid.NewGuid(),
            BatchID = batch.BatchID,
            MovementType = "Batch Created",
            Quantity = batchDto.InitialQuantity,
            MovementDate = DateTime.UtcNow,
            Reference = "Initial Stock"
        };
        
        _context.BatchMovements.Add(movement);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return MapBatchToDTO(batch);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error creating batch: {BatchNumber}", batchDto.BatchNumber);
        throw;
    }
}
```

#### FIFO (First In, First Out) Logic
```csharp
public async Task<List<ProductBatch>> GetBatchesForSaleAsync(Guid productId, int requiredQuantity)
{
    var availableBatches = await _context.ProductBatches
        .Where(b => b.ProductID == productId && 
                   b.CurrentStock > 0 && 
                   b.IsActive &&
                   b.ExpiryDate > DateTime.Now)
        .OrderBy(b => b.ExpiryDate)  // FEFO - First Expired, First Out
        .ThenBy(b => b.CreatedDate)  // Then FIFO
        .ToListAsync();
    
    var selectedBatches = new List<ProductBatch>();
    int remainingQuantity = requiredQuantity;
    
    foreach (var batch in availableBatches)
    {
        if (remainingQuantity <= 0) break;
        
        int quantityFromBatch = Math.Min(remainingQuantity, batch.CurrentStock);
        if (quantityFromBatch > 0)
        {
            selectedBatches.Add(batch);
            remainingQuantity -= quantityFromBatch;
        }
    }
    
    return selectedBatches;
}
```

### Expiry Date Management
```csharp
public async Task<IEnumerable<ExpiryAlertDTO>> GetExpiryAlertsAsync()
{
    var alerts = new List<ExpiryAlertDTO>();
    
    // Get batches expiring in the next 30 days
    var expiringBatches = await _context.ProductBatches
        .Include(b => b.Product)
        .Where(b => b.IsActive && 
                   b.CurrentStock > 0 &&
                   b.ExpiryDate <= DateTime.Now.AddDays(30))
        .OrderBy(b => b.ExpiryDate)
        .ToListAsync();
    
    foreach (var batch in expiringBatches)
    {
        var daysToExpiry = (batch.ExpiryDate - DateTime.Now).Days;
        var alertLevel = GetAlertLevel(daysToExpiry);
        
        alerts.Add(new ExpiryAlertDTO
        {
            BatchID = batch.BatchID,
            ProductID = batch.ProductID,
            ProductName = batch.Product.ProductName,
            BatchNumber = batch.BatchNumber,
            ExpiryDate = batch.ExpiryDate,
            CurrentStock = batch.CurrentStock,
            DaysToExpiry = daysToExpiry,
            AlertLevel = alertLevel,
            EstimatedLoss = batch.CurrentStock * batch.Product.UnitPrice
        });
    }
    
    return alerts;
}

private string GetAlertLevel(int daysToExpiry)
{
    return daysToExpiry switch
    {
        <= 0 => "Expired",
        <= 7 => "Critical",
        <= 14 => "Warning",
        <= 30 => "Notice",
        _ => "Normal"
    };
}
```

## 📊 Stock Management

### Real-Time Stock Tracking
```csharp
public async Task UpdateStockAsync(Guid productId, int quantity, string reason, string movementType = "Adjustment")
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product {productId} not found");
        }
        
        // Validate stock adjustment
        if (movementType == "Decrease" && product.CurrentStock + quantity < 0)
        {
            throw new InvalidOperationException("Insufficient stock for this adjustment");
        }
        
        // Update product stock
        var oldStock = product.CurrentStock;
        product.CurrentStock += quantity;
        product.LastUpdated = DateTime.UtcNow;
        
        // Create stock adjustment record
        var adjustment = new StockAdjustment
        {
            AdjustmentID = Guid.NewGuid(),
            ProductID = productId,
            AdjustmentType = movementType,
            Quantity = Math.Abs(quantity),
            Reason = reason,
            AdjustmentDate = DateTime.UtcNow,
            AdjustedBy = _currentUserService.GetCurrentUserId()
        };
        
        _context.StockAdjustments.Add(adjustment);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        _logger.LogInformation("Stock updated for product {ProductID}: {OldStock} -> {NewStock} ({Reason})", 
            productId, oldStock, product.CurrentStock, reason);
        
        // Check for low stock alerts
        await CheckLowStockAlertAsync(product);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error updating stock for product {ProductID}", productId);
        throw;
    }
}
```

### Automated Reorder Management
```csharp
public async Task<IEnumerable<ReorderSuggestionDTO>> GetReorderSuggestionsAsync()
{
    var lowStockProducts = await _context.Products
        .Include(p => p.Supplier)
        .Where(p => p.IsActive && p.CurrentStock <= p.ReorderLevel)
        .ToListAsync();
    
    var suggestions = new List<ReorderSuggestionDTO>();
    
    foreach (var product in lowStockProducts)
    {
        // Calculate suggested order quantity
        var averageDailyUsage = await CalculateAverageDailyUsageAsync(product.ProductID);
        var leadTimeDays = product.Supplier?.LeadTimeDays ?? 7;
        var safetyStock = (int)(averageDailyUsage * leadTimeDays * 1.2); // 20% safety margin
        
        var suggestedQuantity = Math.Max(
            product.MaxStockLevel - product.CurrentStock,
            safetyStock
        );
        
        suggestions.Add(new ReorderSuggestionDTO
        {
            ProductID = product.ProductID,
            ProductName = product.ProductName,
            CurrentStock = product.CurrentStock,
            ReorderLevel = product.ReorderLevel,
            MaxStockLevel = product.MaxStockLevel,
            SuggestedOrderQuantity = suggestedQuantity,
            SupplierName = product.Supplier?.SupplierName,
            EstimatedCost = suggestedQuantity * (product.Supplier?.LastPurchasePrice ?? product.UnitPrice * 0.7m),
            Priority = CalculateReorderPriority(product.CurrentStock, product.ReorderLevel, averageDailyUsage)
        });
    }
    
    return suggestions.OrderByDescending(s => s.Priority).ToList();
}
```

### Stock Movement Tracking
```csharp
public class BatchMovement
{
    public Guid MovementID { get; set; }
    public Guid BatchID { get; set; }
    public string MovementType { get; set; } // Sale, Purchase, Adjustment, Transfer, Expired
    public int Quantity { get; set; }
    public DateTime MovementDate { get; set; }
    public string Reference { get; set; } // Sale ID, Purchase ID, etc.
    public string Notes { get; set; }
    
    // Navigation Properties
    public ProductBatch ProductBatch { get; set; }
}
```

## 🏷️ Barcode Management

### Barcode Generation
```csharp
public async Task<string> GenerateBarcodeAsync()
{
    string barcode;
    bool isUnique;
    
    do
    {
        // Generate EAN-13 barcode
        var random = new Random();
        var countryCode = "123"; // Pharmacy-specific prefix
        var manufacturerCode = random.Next(10000, 99999).ToString();
        var productCode = random.Next(100, 999).ToString();
        
        var partialBarcode = countryCode + manufacturerCode + productCode;
        var checkDigit = CalculateEAN13CheckDigit(partialBarcode);
        barcode = partialBarcode + checkDigit;
        
        // Check if barcode is unique
        isUnique = !await _context.Products.AnyAsync(p => p.Barcode == barcode);
        
    } while (!isUnique);
    
    return barcode;
}

private int CalculateEAN13CheckDigit(string barcode)
{
    var sum = 0;
    for (int i = 0; i < barcode.Length; i++)
    {
        var digit = int.Parse(barcode[i].ToString());
        sum += (i % 2 == 0) ? digit : digit * 3;
    }
    
    var checkDigit = (10 - (sum % 10)) % 10;
    return checkDigit;
}
```

### Barcode Scanning Integration
```javascript
// Frontend barcode scanning
function initializeBarcodeScanner() {
    const scanner = new Html5QrcodeScanner("barcode-scanner", {
        fps: 10,
        qrbox: { width: 250, height: 250 }
    });
    
    scanner.render(onScanSuccess, onScanFailure);
}

function onScanSuccess(decodedText, decodedResult) {
    // Call API to find product by barcode
    findProductByBarcode(decodedText)
        .then(product => {
            if (product) {
                addProductToCart(product);
            } else {
                showError('Product not found');
            }
        })
        .catch(error => {
            showError('Error scanning barcode: ' + error.message);
        });
}
```

## 📊 Inventory Reports & Analytics

### Inventory Valuation Report
```csharp
public async Task<InventoryValuationReportDTO> GenerateInventoryValuationReportAsync()
{
    var products = await _context.Products
        .Include(p => p.ProductBatches)
        .Where(p => p.IsActive)
        .ToListAsync();
    
    var report = new InventoryValuationReportDTO
    {
        GeneratedDate = DateTime.UtcNow,
        TotalProducts = products.Count,
        Categories = new List<CategoryValuationDTO>()
    };
    
    var categories = products.GroupBy(p => p.Category);
    
    foreach (var category in categories)
    {
        var categoryValuation = new CategoryValuationDTO
        {
            CategoryName = category.Key,
            ProductCount = category.Count(),
            TotalQuantity = category.Sum(p => p.CurrentStock),
            TotalValue = 0
        };
        
        foreach (var product in category)
        {
            var productValue = 0m;
            foreach (var batch in product.ProductBatches.Where(b => b.IsActive && b.CurrentStock > 0))
            {
                productValue += batch.CurrentStock * batch.CostPrice;
            }
            categoryValuation.TotalValue += productValue;
        }
        
        report.Categories.Add(categoryValuation);
    }
    
    report.TotalInventoryValue = report.Categories.Sum(c => c.TotalValue);
    return report;
}
```

### ABC Analysis
```csharp
public async Task<ABCAnalysisReportDTO> GenerateABCAnalysisAsync()
{
    // Get sales data for the last 12 months
    var startDate = DateTime.Now.AddMonths(-12);
    var salesData = await _context.SaleItems
        .Include(si => si.Product)
        .Where(si => si.Sale.SaleDate >= startDate)
        .GroupBy(si => si.ProductID)
        .Select(g => new ProductSalesDTO
        {
            ProductID = g.Key,
            ProductName = g.First().Product.ProductName,
            TotalQuantitySold = g.Sum(si => si.Quantity),
            TotalRevenue = g.Sum(si => si.LineTotal),
            CurrentStock = g.First().Product.CurrentStock
        })
        .OrderByDescending(p => p.TotalRevenue)
        .ToListAsync();
    
    var totalRevenue = salesData.Sum(p => p.TotalRevenue);
    var runningPercentage = 0m;
    var categoryA = new List<ProductSalesDTO>();
    var categoryB = new List<ProductSalesDTO>();
    var categoryC = new List<ProductSalesDTO>();
    
    foreach (var product in salesData)
    {
        var productPercentage = (product.TotalRevenue / totalRevenue) * 100;
        runningPercentage += productPercentage;
        
        if (runningPercentage <= 70)
        {
            categoryA.Add(product);
        }
        else if (runningPercentage <= 90)
        {
            categoryB.Add(product);
        }
        else
        {
            categoryC.Add(product);
        }
    }
    
    return new ABCAnalysisReportDTO
    {
        CategoryA = categoryA, // High value items (70% of revenue)
        CategoryB = categoryB, // Medium value items (20% of revenue)
        CategoryC = categoryC, // Low value items (10% of revenue)
        AnalysisDate = DateTime.UtcNow
    };
}
```

## 🎮 User Interface Guide

### Product List Interface
- **Search and Filter**: Product name, category, manufacturer, barcode
- **Sort Options**: Name, category, stock level, price, expiry date
- **Action Buttons**: Add, Edit, Delete, View Details, Add Stock
- **Quick Actions**: Mark as inactive, duplicate product, print labels

### Product Creation Form
1. **Basic Information**: Name, description, category, manufacturer
2. **Pricing**: Unit price, cost price (for batches)
3. **Stock Settings**: Reorder level, max stock level, unit of measure
4. **Classification**: Controlled substance, prescription requirement
5. **Supplier**: Primary supplier selection
6. **Barcode**: Auto-generated or manual entry

### Batch Management Interface
- **Batch List**: All batches for a product with expiry status
- **Add Batch**: Create new batch with manufacture/expiry dates
- **Stock Movements**: View all movements for a batch
- **Expiry Alerts**: Visual indicators for near-expiry batches

### Inventory Dashboard
- **Stock Levels**: Current stock vs. reorder levels
- **Low Stock Alerts**: Products requiring reorder
- **Expiry Alerts**: Products/batches nearing expiry
- **Valuation Summary**: Total inventory value by category
- **Quick Actions**: Stock adjustment, reorder products

## 🔧 Configuration & Settings

### Stock Alert Settings
```json
{
  "StockAlertSettings": {
    "LowStockNotificationEnabled": true,
    "ExpiryAlertDays": [7, 14, 30],
    "AutoReorderEnabled": false,
    "StockValuationMethod": "FIFO",
    "DefaultReorderQuantityDays": 30,
    "SafetyStockPercentage": 20
  }
}
```

### Product Categories Configuration
```json
{
  "ProductCategories": {
    "PrescriptionMedications": {
      "RequiresPrescription": true,
      "DefaultTaxRate": 0.0,
      "SubCategories": ["Antibiotics", "Cardiovascular", "Diabetes"]
    },
    "OTCMedications": {
      "RequiresPrescription": false,
      "DefaultTaxRate": 0.05,
      "SubCategories": ["PainRelief", "ColdFlu", "Vitamins"]
    }
  }
}
```

## 🚀 Advanced Features

### Multi-Location Support
```csharp
public class ProductLocation
{
    public Guid LocationID { get; set; }
    public Guid ProductID { get; set; }
    public string LocationName { get; set; }
    public int StockQuantity { get; set; }
    public string ShelfLocation { get; set; }
    public bool IsMainLocation { get; set; }
}
```

### Automated Stock Replenishment
```csharp
public async Task ProcessAutomaticReorderAsync()
{
    var reorderSuggestions = await GetReorderSuggestionsAsync();
    
    foreach (var suggestion in reorderSuggestions.Where(s => s.Priority == "High"))
    {
        var product = await _context.Products
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.ProductID == suggestion.ProductID);
        
        if (product?.Supplier != null && product.Supplier.AutoReorderEnabled)
        {
            await CreateAutomaticPurchaseOrderAsync(product, suggestion.SuggestedOrderQuantity);
        }
    }
}
```

### Product Image Management
```csharp
public async Task<string> UploadProductImageAsync(Guid productId, IFormFile imageFile)
{
    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
    var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
    
    if (!allowedExtensions.Contains(extension))
    {
        throw new InvalidOperationException("Invalid image format");
    }
    
    var fileName = $"{productId}_{Guid.NewGuid()}{extension}";
    var filePath = Path.Combine("wwwroot", "images", "products", fileName);
    
    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await imageFile.CopyToAsync(stream);
    }
    
    // Update product with image path
    var product = await _context.Products.FindAsync(productId);
    product.ImagePath = $"/images/products/{fileName}";
    await _context.SaveChangesAsync();
    
    return product.ImagePath;
}
```

## 🔒 Security & Compliance

### Access Control
- **Product Creation**: Managers and above
- **Price Changes**: Managers and above
- **Stock Adjustments**: All staff (with audit trail)
- **Batch Management**: Pharmacists and above
- **Inventory Reports**: All staff (read-only)

### Audit Trail
```csharp
public class ProductAuditLog
{
    public Guid AuditID { get; set; }
    public Guid ProductID { get; set; }
    public string Action { get; set; } // Created, Updated, Deleted, StockAdjusted
    public string OldValues { get; set; } // JSON
    public string NewValues { get; set; } // JSON
    public Guid PerformedBy { get; set; }
    public DateTime Timestamp { get; set; }
    public string IPAddress { get; set; }
    public string UserAgent { get; set; }
}
```

### Controlled Substance Tracking
```csharp
public async Task LogControlledSubstanceMovementAsync(Guid productId, int quantity, 
    string movementType, string reference)
{
    var product = await _context.Products.FindAsync(productId);
    
    if (product.IsControlledSubstance)
    {
        var log = new ControlledSubstanceLog
        {
            LogID = Guid.NewGuid(),
            ProductID = productId,
            MovementType = movementType,
            Quantity = quantity,
            Reference = reference,
            Timestamp = DateTime.UtcNow,
            UserID = _currentUserService.GetCurrentUserId(),
            RequiresReporting = true
        };
        
        _context.ControlledSubstanceLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
```

## 🐛 Troubleshooting

### Common Issues

#### Stock Discrepancies
```csharp
public async Task<StockReconciliationReportDTO> ReconcileStockAsync(Guid productId)
{
    var product = await _context.Products
        .Include(p => p.ProductBatches)
        .FirstOrDefaultAsync(p => p.ProductID == productId);
    
    var systemStock = product.CurrentStock;
    var batchStock = product.ProductBatches.Where(b => b.IsActive).Sum(b => b.CurrentStock);
    
    var discrepancy = systemStock - batchStock;
    
    if (discrepancy != 0)
    {
        _logger.LogWarning("Stock discrepancy found for product {ProductID}: System={SystemStock}, Batches={BatchStock}", 
            productId, systemStock, batchStock);
        
        // Auto-correct if within tolerance
        if (Math.Abs(discrepancy) <= 5)
        {
            product.CurrentStock = batchStock;
            await _context.SaveChangesAsync();
        }
    }
    
    return new StockReconciliationReportDTO
    {
        ProductID = productId,
        SystemStock = systemStock,
        BatchStock = batchStock,
        Discrepancy = discrepancy,
        RecommendedAction = Math.Abs(discrepancy) > 5 ? "Manual Investigation Required" : "Auto-Corrected"
    };
}
```

#### Performance Optimization
- Index product searches by barcode and name
- Cache frequently accessed product lists
- Optimize batch queries with proper joins
- Use pagination for large product lists

---

> 💡 **Inventory Excellence**: The Products & Inventory module provides complete control over your pharmacy's stock, ensuring accuracy, compliance, and efficiency in all inventory operations.

**Need Help?** Check the [User Guides](../user-guides/Pharmacist-Guide.md) for detailed operational procedures or the [Admin Guide](../user-guides/Admin-Guide.md) for system configuration.