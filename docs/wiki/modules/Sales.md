# 🛍️ Sales Management Module

Complete guide for the Sales Management module including point-of-sale operations, invoice generation, and sales analytics.

## 🎯 Module Overview

The Sales Management module handles all sales transactions, customer interactions, and revenue tracking in the pharmacy system.

### Key Features
- **Point of Sale (POS)**: Quick and efficient sales processing
- **Invoice Generation**: Professional invoice creation and printing
- **Customer Management**: Customer tracking and purchase history
- **Payment Processing**: Multiple payment method support
- **Sales Analytics**: Comprehensive sales reporting and insights
- **Inventory Integration**: Automatic stock updates on sales

### Business Value
- **Faster Checkout**: Streamlined POS interface reduces transaction time
- **Customer Service**: Complete purchase history and customer preferences
- **Revenue Tracking**: Real-time sales analytics and reporting
- **Compliance**: Proper invoicing and audit trails
- **Inventory Control**: Automatic stock deductions and alerts

## 🔧 Technical Architecture

### Data Models
```csharp
// Sale Entity
public class Sale
{
    public Guid SaleID { get; set; }
    public Guid? CustomerID { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string PaymentMethod { get; set; }
    public string SaleStatus { get; set; }
    public string Notes { get; set; }
    public Guid UserID { get; set; }
    
    // Navigation Properties
    public Customer Customer { get; set; }
    public User User { get; set; }
    public List<SaleItem> SaleItems { get; set; }
}

// Sale Item Entity
public class SaleItem
{
    public Guid SaleItemID { get; set; }
    public Guid SaleID { get; set; }
    public Guid ProductID { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    
    // Navigation Properties
    public Sale Sale { get; set; }
    public Product Product { get; set; }
}
```

### Service Layer
```csharp
public interface ISalesService
{
    Task<IEnumerable<SaleDTO>> GetAllSalesAsync();
    Task<SaleDTO> GetSaleByIdAsync(Guid saleId);
    Task<SaleDTO> CreateSaleAsync(SaleDTO saleDto);
    Task<SaleDTO> UpdateSaleAsync(SaleDTO saleDto);
    Task<bool> DeleteSaleAsync(Guid saleId);
    Task<IEnumerable<SaleDTO>> GetSalesByCustomerAsync(Guid customerId);
    Task<IEnumerable<SaleDTO>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalSalesAmountAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<TopProductDTO>> GetTopSellingProductsAsync(int count = 10);
    Task<byte[]> GenerateInvoicePdfAsync(Guid saleId);
}
```

## 🛒 Point of Sale (POS) Operations

### POS Interface Features
- **Product Search**: Quick product lookup by name, barcode, or category
- **Customer Selection**: Link sales to customers for tracking
- **Multiple Payment Methods**: Cash, Card, Insurance, Mixed payments
- **Discount Application**: Item-level and transaction-level discounts
- **Real-time Calculations**: Automatic totals, tax, and change calculations
- **Prescription Handling**: Special handling for prescription medications

### Creating a Sale
```csharp
public async Task<SaleDTO> CreateSaleAsync(SaleDTO saleDto)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // 1. Validate sale data
        await ValidateSaleDataAsync(saleDto);
        
        // 2. Check product availability
        await ValidateProductAvailabilityAsync(saleDto.SaleItems);
        
        // 3. Create sale entity
        var sale = new Sale
        {
            SaleID = Guid.NewGuid(),
            CustomerID = saleDto.CustomerID,
            SaleDate = DateTime.UtcNow,
            PaymentMethod = saleDto.PaymentMethod,
            Notes = saleDto.Notes,
            UserID = saleDto.UserID,
            SaleStatus = "Completed"
        };
        
        // 4. Process sale items
        decimal totalAmount = 0;
        foreach (var itemDto in saleDto.SaleItems)
        {
            var saleItem = new SaleItem
            {
                SaleItemID = Guid.NewGuid(),
                SaleID = sale.SaleID,
                ProductID = itemDto.ProductID,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                DiscountAmount = itemDto.DiscountAmount
            };
            
            saleItem.LineTotal = (saleItem.Quantity * saleItem.UnitPrice) - saleItem.DiscountAmount;
            totalAmount += saleItem.LineTotal;
            
            sale.SaleItems.Add(saleItem);
            
            // Update product stock
            await UpdateProductStockAsync(itemDto.ProductID, itemDto.Quantity);
        }
        
        // 5. Calculate totals
        sale.TotalAmount = totalAmount - saleDto.DiscountAmount;
        sale.DiscountAmount = saleDto.DiscountAmount;
        sale.TaxAmount = CalculateTax(sale.TotalAmount);
        
        // 6. Save to database
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        // 7. Generate invoice
        await GenerateInvoiceAsync(sale.SaleID);
        
        return MapToDTO(sale);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error creating sale");
        throw;
    }
}
```

### Stock Management Integration
```csharp
private async Task UpdateProductStockAsync(Guid productId, int quantitySold)
{
    // Get available batches (FIFO - First In, First Out)
    var batches = await _context.ProductBatches
        .Where(b => b.ProductID == productId && b.CurrentStock > 0)
        .OrderBy(b => b.ExpiryDate)
        .ThenBy(b => b.CreatedDate)
        .ToListAsync();
    
    int remainingQuantity = quantitySold;
    
    foreach (var batch in batches)
    {
        if (remainingQuantity <= 0) break;
        
        int quantityFromBatch = Math.Min(remainingQuantity, batch.CurrentStock);
        batch.CurrentStock -= quantityFromBatch;
        remainingQuantity -= quantityFromBatch;
        
        // Log batch usage
        var batchLog = new BatchUsageLog
        {
            BatchID = batch.BatchID,
            ProductID = productId,
            QuantityUsed = quantityFromBatch,
            UsageDate = DateTime.UtcNow,
            UsageType = "Sale"
        };
        
        _context.BatchUsageLogs.Add(batchLog);
    }
    
    if (remainingQuantity > 0)
    {
        throw new InvalidOperationException($"Insufficient stock for product {productId}. Missing {remainingQuantity} units.");
    }
}
```

## 🧾 Invoice Management

### Invoice Generation
```csharp
public async Task<byte[]> GenerateInvoicePdfAsync(Guid saleId)
{
    var sale = await GetSaleWithDetailsAsync(saleId);
    
    using var document = new PdfDocument();
    var page = document.AddPage();
    var graphics = XGraphics.FromPdfPage(page);
    var font = new XFont("Arial", 12);
    var titleFont = new XFont("Arial", 16, XFontStyle.Bold);
    
    // Invoice Header
    graphics.DrawString("PHARMACY INVOICE", titleFont, XBrushes.Black, 
        new XPoint(200, 50));
    
    // Pharmacy Information
    graphics.DrawString("ABC Pharmacy", font, XBrushes.Black, 
        new XPoint(50, 100));
    graphics.DrawString("123 Medical Street", font, XBrushes.Black, 
        new XPoint(50, 120));
    graphics.DrawString("Phone: (555) 123-4567", font, XBrushes.Black, 
        new XPoint(50, 140));
    
    // Invoice Details
    graphics.DrawString($"Invoice #: {sale.SaleID}", font, XBrushes.Black, 
        new XPoint(400, 100));
    graphics.DrawString($"Date: {sale.SaleDate:MM/dd/yyyy}", font, XBrushes.Black, 
        new XPoint(400, 120));
    graphics.DrawString($"Cashier: {sale.User.FirstName} {sale.User.LastName}", 
        font, XBrushes.Black, new XPoint(400, 140));
    
    // Customer Information
    if (sale.Customer != null)
    {
        graphics.DrawString("Bill To:", font, XBrushes.Black, 
            new XPoint(50, 180));
        graphics.DrawString(sale.Customer.CustomerName, font, XBrushes.Black, 
            new XPoint(50, 200));
        graphics.DrawString(sale.Customer.Phone ?? "", font, XBrushes.Black, 
            new XPoint(50, 220));
    }
    
    // Items Table
    int yPosition = 260;
    graphics.DrawString("Description", font, XBrushes.Black, new XPoint(50, yPosition));
    graphics.DrawString("Qty", font, XBrushes.Black, new XPoint(300, yPosition));
    graphics.DrawString("Price", font, XBrushes.Black, new XPoint(350, yPosition));
    graphics.DrawString("Total", font, XBrushes.Black, new XPoint(450, yPosition));
    
    yPosition += 20;
    foreach (var item in sale.SaleItems)
    {
        graphics.DrawString(item.Product.ProductName, font, XBrushes.Black, 
            new XPoint(50, yPosition));
        graphics.DrawString(item.Quantity.ToString(), font, XBrushes.Black, 
            new XPoint(300, yPosition));
        graphics.DrawString($"${item.UnitPrice:F2}", font, XBrushes.Black, 
            new XPoint(350, yPosition));
        graphics.DrawString($"${item.LineTotal:F2}", font, XBrushes.Black, 
            new XPoint(450, yPosition));
        yPosition += 20;
    }
    
    // Totals
    yPosition += 20;
    graphics.DrawString($"Subtotal: ${sale.TotalAmount:F2}", font, XBrushes.Black, 
        new XPoint(350, yPosition));
    yPosition += 20;
    graphics.DrawString($"Discount: ${sale.DiscountAmount:F2}", font, XBrushes.Black, 
        new XPoint(350, yPosition));
    yPosition += 20;
    graphics.DrawString($"Tax: ${sale.TaxAmount:F2}", font, XBrushes.Black, 
        new XPoint(350, yPosition));
    yPosition += 20;
    graphics.DrawString($"Total: ${(sale.TotalAmount + sale.TaxAmount):F2}", 
        titleFont, XBrushes.Black, new XPoint(350, yPosition));
    
    // Footer
    graphics.DrawString("Thank you for your business!", font, XBrushes.Black, 
        new XPoint(200, yPosition + 60));
    
    using var stream = new MemoryStream();
    document.Save(stream);
    return stream.ToArray();
}
```

### Invoice Templates
The system supports multiple invoice templates:
- **Standard Invoice**: Basic invoice with all required information
- **Prescription Invoice**: Special format for prescription medications
- **Insurance Invoice**: Format for insurance claims
- **Receipt**: Simple receipt format for quick transactions

## 📊 Sales Analytics & Reporting

### Dashboard Analytics
```csharp
public async Task<DashboardKPIDTO> GetSalesKPIsAsync()
{
    var today = DateTime.Today;
    var yesterday = today.AddDays(-1);
    var thisMonth = new DateTime(today.Year, today.Month, 1);
    var lastMonth = thisMonth.AddMonths(-1);
    
    return new DashboardKPIDTO
    {
        TodaySales = await GetTotalSalesAmountAsync(today, today.AddDays(1)),
        YesterdaySales = await GetTotalSalesAmountAsync(yesterday, today),
        ThisMonthSales = await GetTotalSalesAmountAsync(thisMonth, DateTime.Now),
        LastMonthSales = await GetTotalSalesAmountAsync(lastMonth, thisMonth),
        TotalTransactionsToday = await GetTransactionCountAsync(today, today.AddDays(1)),
        AverageTransactionValue = await GetAverageTransactionValueAsync(),
        TopSellingProducts = await GetTopSellingProductsAsync(5),
        SalesGrowthPercentage = await CalculateSalesGrowthAsync()
    };
}
```

### Sales Reports
1. **Daily Sales Report**: Transaction summary for a specific day
2. **Monthly Sales Report**: Comprehensive monthly performance
3. **Product Performance Report**: Best and worst performing products
4. **Customer Purchase Report**: Customer buying patterns
5. **Cashier Performance Report**: Individual staff performance
6. **Payment Method Analysis**: Payment method preferences

### Advanced Analytics
```csharp
public async Task<SalesAnalyticsDTO> GetAdvancedSalesAnalyticsAsync(
    DateTime startDate, DateTime endDate)
{
    var sales = await _context.Sales
        .Include(s => s.SaleItems)
        .ThenInclude(si => si.Product)
        .Include(s => s.Customer)
        .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
        .ToListAsync();
    
    return new SalesAnalyticsDTO
    {
        TotalRevenue = sales.Sum(s => s.TotalAmount),
        TotalTransactions = sales.Count,
        AverageTransactionValue = sales.Average(s => s.TotalAmount),
        
        // Time-based analysis
        HourlyDistribution = GetHourlyDistribution(sales),
        DailyDistribution = GetDailyDistribution(sales),
        
        // Product analysis
        TopProducts = GetTopProducts(sales, 10),
        CategoryPerformance = GetCategoryPerformance(sales),
        
        // Customer analysis
        NewCustomers = GetNewCustomersCount(sales, startDate),
        RepeatCustomers = GetRepeatCustomersCount(sales),
        
        // Payment analysis
        PaymentMethodBreakdown = GetPaymentMethodBreakdown(sales)
    };
}
```

## 🎮 User Interface Guide

### POS Interface
The Point of Sale interface is designed for speed and efficiency:

1. **Product Search Bar**: Quick product lookup
2. **Shopping Cart**: Shows selected items and running total
3. **Customer Selection**: Optional customer association
4. **Payment Panel**: Payment method selection and amount entry
5. **Quick Actions**: Common operations like discounts and refunds

### Key Shortcuts
- **F1**: New Sale
- **F2**: Find Product
- **F3**: Find Customer
- **F4**: Apply Discount
- **F5**: Refresh Product List
- **F9**: Payment
- **F12**: Complete Sale

### Sales List Interface
- **Filter Options**: Date range, customer, payment method, cashier
- **Sort Options**: Date, amount, customer name
- **Action Buttons**: View details, print invoice, refund
- **Export Options**: PDF, Excel, CSV

## 🔧 Configuration & Settings

### Payment Methods
Configure accepted payment methods:
- **Cash**: Standard cash payments
- **Credit Card**: Card payment processing
- **Debit Card**: Debit card processing
- **Insurance**: Insurance claim processing
- **Check**: Check payments (if accepted)
- **Mixed**: Combination of payment methods

### Tax Configuration
```json
{
  "TaxSettings": {
    "DefaultTaxRate": 0.08,
    "TaxExemptProducts": ["prescription-medications"],
    "TaxByCategory": {
      "OTC-Medications": 0.05,
      "Cosmetics": 0.10,
      "Food-Supplements": 0.08
    }
  }
}
```

### Discount Rules
- **Percentage Discounts**: Apply percentage off items or total
- **Fixed Amount Discounts**: Apply fixed dollar amount off
- **BOGO Offers**: Buy One Get One promotions
- **Quantity Discounts**: Bulk purchase discounts
- **Customer-Specific Discounts**: Special customer pricing

## 🚀 Advanced Features

### Prescription Management
Special handling for prescription medications:
- **Prescription Verification**: Verify prescription validity
- **Controlled Substances**: Special tracking for controlled medications
- **Insurance Processing**: Automatic insurance claim submission
- **Refill Tracking**: Track prescription refill limits

### Customer Loyalty Program
```csharp
public async Task ApplyLoyaltyDiscountAsync(Guid customerId, SaleDTO sale)
{
    var customer = await _context.Customers.FindAsync(customerId);
    var loyaltyPoints = await CalculateLoyaltyPointsAsync(customer, sale.TotalAmount);
    
    // Apply loyalty discount if eligible
    if (loyaltyPoints >= 100)
    {
        var discount = Math.Min(loyaltyPoints / 100 * 5, sale.TotalAmount * 0.10m);
        sale.DiscountAmount += discount;
        customer.LoyaltyPoints -= (loyaltyPoints / 100) * 100;
    }
    
    // Add new loyalty points
    customer.LoyaltyPoints += (int)(sale.TotalAmount / 10);
    await _context.SaveChangesAsync();
}
```

### Return & Refund Processing
```csharp
public async Task<RefundDTO> ProcessRefundAsync(RefundRequestDTO refundRequest)
{
    var originalSale = await GetSaleByIdAsync(refundRequest.OriginalSaleId);
    
    // Validate refund eligibility
    if (DateTime.Now - originalSale.SaleDate > TimeSpan.FromDays(30))
    {
        throw new InvalidOperationException("Refund period has expired");
    }
    
    // Create refund transaction
    var refund = new Sale
    {
        SaleID = Guid.NewGuid(),
        CustomerID = originalSale.CustomerID,
        SaleDate = DateTime.UtcNow,
        TotalAmount = -refundRequest.RefundAmount,
        PaymentMethod = "Refund",
        SaleStatus = "Refunded",
        Notes = $"Refund for sale {originalSale.SaleID}",
        UserID = refundRequest.ProcessedByUserId
    };
    
    // Restore inventory
    foreach (var item in refundRequest.RefundItems)
    {
        await RestoreProductStockAsync(item.ProductID, item.Quantity);
    }
    
    await _context.Sales.AddAsync(refund);
    await _context.SaveChangesAsync();
    
    return MapRefundToDTO(refund);
}
```

## 📈 Performance Optimization

### Database Optimization
- **Indexed Queries**: Proper indexing on SaleDate, CustomerID, ProductID
- **Batch Operations**: Bulk insert for large sales
- **Connection Pooling**: Efficient database connection management
- **Query Optimization**: Optimized LINQ queries for reporting

### Caching Strategy
```csharp
public async Task<IEnumerable<ProductDTO>> GetFrequentlyPurchasedProductsAsync()
{
    const string cacheKey = "frequently_purchased_products";
    
    if (!_cache.TryGetValue(cacheKey, out IEnumerable<ProductDTO> products))
    {
        products = await _productService.GetTopSellingProductsAsync(50);
        
        _cache.Set(cacheKey, products, TimeSpan.FromMinutes(30));
    }
    
    return products;
}
```

## 🔒 Security & Compliance

### Access Control
- **Role-Based Access**: Different permissions for cashiers, managers, admins
- **Transaction Limits**: Daily transaction limits for cashiers
- **Audit Logging**: Complete audit trail for all sales transactions
- **Data Encryption**: Sensitive customer data encryption

### Compliance Requirements
- **HIPAA Compliance**: Protected health information handling
- **PCI DSS**: Credit card data security standards
- **FDA Regulations**: Prescription medication tracking
- **State Regulations**: Local pharmacy regulations compliance

## 🐛 Troubleshooting

### Common Issues

#### "Insufficient Stock" Error
```csharp
// Check available stock before sale
var availableStock = await _stockService.GetAvailableStockAsync(productId);
if (requestedQuantity > availableStock)
{
    throw new InsufficientStockException(
        $"Only {availableStock} units available for {productName}");
}
```

#### Payment Processing Failures
- Verify payment gateway configuration
- Check network connectivity
- Validate payment method setup
- Review transaction logs

#### Invoice Generation Issues
- Verify PDF library installation
- Check file system permissions
- Validate invoice template format
- Review printer configuration

### Performance Issues
- Monitor database query performance
- Check for N+1 query problems
- Review caching effectiveness
- Analyze slow transaction queries

---

> 💡 **Sales Success**: The Sales Management module provides everything needed for efficient pharmacy sales operations, from quick POS transactions to comprehensive analytics and reporting.

**Need Help?** Check the [User Guides](../user-guides/Pharmacist-Guide.md) for detailed operational procedures or the [Admin Guide](../user-guides/Admin-Guide.md) for system configuration.