# 📦 Purchase Orders Module

The Purchase Orders module manages the complete procurement workflow from order creation to inventory integration.

## 🎯 Module Overview

The Purchase Orders system handles the entire procurement lifecycle with advanced status management and automatic inventory integration.

### Key Features
- ✅ **Complete Workflow**: Pending → Processed → Approved → Cancelled
- ✅ **Inventory Integration**: Automatic stock level updates
- ✅ **Supplier Management**: Link orders to vendor records
- ✅ **Status Tracking**: Real-time order status monitoring
- ✅ **Professional UI**: Modern interface with action buttons
- ✅ **Validation & Error Handling**: Comprehensive business rules

## 🔄 Purchase Order Workflow

### Status Lifecycle
```
📝 Created (Pending)
    ↓
🔄 Processing (In Progress)
    ↓
✅ Processed (Stock Updated)
    ↓
👍 Approved (Final Approval)
    ↓
📊 Completed (Closed)

❌ Cancelled (Any Stage)
```

### Business Rules
- Orders start in **Pending** status
- Only **Pending** orders can be processed
- **Processing** updates inventory automatically
- **Approval** finalizes the order
- **Cancellation** requires a reason

## 🏗️ Technical Architecture

### Service Layer
```csharp
public interface IPurchaseOrderService
{
    // CRUD Operations
    Task<IEnumerable<PurchaseOrderDTO>> GetAllPurchaseOrdersAsync();
    Task<PurchaseOrderDTO> GetPurchaseOrderByIdAsync(Guid id);
    Task<PurchaseOrderDTO> CreatePurchaseOrderAsync(PurchaseOrderDTO order);
    Task<bool> UpdatePurchaseOrderAsync(PurchaseOrderDTO order);
    Task<bool> DeletePurchaseOrderAsync(Guid id);
    
    // Status Management
    Task<bool> ProcessPurchaseOrderAsync(Guid orderId, Guid userId);
    Task<bool> ApprovePurchaseOrderAsync(Guid orderId, Guid userId);
    Task<bool> CancelPurchaseOrderAsync(Guid orderId, string reason);
    
    // Business Operations
    Task<decimal> CalculateOrderTotalAsync(Guid orderId);
    Task<IEnumerable<PurchaseOrderDTO>> GetOrdersByStatusAsync(string status);
    Task<IEnumerable<PurchaseOrderDTO>> GetOrdersBySupplierAsync(Guid supplierId);
}
```

### Data Model
```csharp
public class PurchaseOrder
{
    public Guid PurchaseOrderID { get; set; }
    public string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } // Pending, Processed, Approved, Cancelled
    public decimal TotalAmount { get; set; }
    public string Notes { get; set; }
    public string CancellationReason { get; set; }
    
    // Relationships
    public Guid SupplierID { get; set; }
    public Supplier Supplier { get; set; }
    public ICollection<PurchaseOrderItem> Items { get; set; }
    
    // Audit Fields
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}

public class PurchaseOrderItem
{
    public Guid PurchaseOrderItemID { get; set; }
    public Guid PurchaseOrderID { get; set; }
    public Guid ProductID { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string BatchNumber { get; set; }
    
    // Navigation Properties
    public PurchaseOrder PurchaseOrder { get; set; }
    public Product Product { get; set; }
    public Guid? ProductBatchID { get; set; } // Nullable - created during processing
    public ProductBatch ProductBatch { get; set; }
}
```

## 🎨 User Interface

### Index Page Features
- **Order List**: Sortable table with advanced filtering
- **Status Badges**: Color-coded status indicators
- **Action Buttons**: Context-sensitive operations
- **Search & Filter**: Find orders by various criteria
- **Export Options**: Data export capabilities

### Action Buttons by Status
| Status | Available Actions |
|--------|------------------|
| **Pending** | Process, Cancel |
| **Processed** | Approve, Cancel |
| **Approved** | View Only |
| **Cancelled** | View Only |

### Status Badge Colors
```css
.status-pending { background: #ffc107; } /* Yellow */
.status-processed { background: #17a2b8; } /* Info Blue */
.status-approved { background: #28a745; } /* Green */
.status-cancelled { background: #dc3545; } /* Red */
```

## 🔧 Key Business Operations

### 1. Order Processing (Most Important)
```csharp
public async Task<bool> ProcessPurchaseOrderAsync(Guid orderId, Guid userId)
{
    var order = await _repository.GetByIdAsync(orderId);
    
    // Validation
    if (order == null || order.Status != "Pending")
        return false;
    
    // Update order status
    order.Status = "Processed";
    order.UpdatedDate = DateTime.Now;
    order.UpdatedBy = userId;
    
    // Process each item and update inventory
    foreach (var item in order.Items)
    {
        await ProcessOrderItem(item);
    }
    
    await _repository.UpdateAsync(order);
    return true;
}

private async Task ProcessOrderItem(PurchaseOrderItem item)
{
    // Find existing batch or create new one
    var batch = await _productBatchRepository
        .GetByProductAndBatchAsync(item.ProductID, item.BatchNumber);
    
    if (batch == null)
    {
        // Create new batch
        batch = new ProductBatch
        {
            ProductBatchID = Guid.NewGuid(),
            ProductID = item.ProductID,
            BatchNumber = item.BatchNumber,
            ExpiryDate = item.ExpiryDate,
            Quantity = item.Quantity,
            CostPrice = item.UnitPrice,
            CreatedDate = DateTime.Now
        };
        await _productBatchRepository.CreateAsync(batch);
    }
    else
    {
        // Update existing batch
        batch.Quantity += item.Quantity;
        batch.UpdatedDate = DateTime.Now;
        await _productBatchRepository.UpdateAsync(batch);
    }
    
    // Link item to batch
    item.ProductBatchID = batch.ProductBatchID;
}
```

### 2. Order Approval
```csharp
public async Task<bool> ApprovePurchaseOrderAsync(Guid orderId, Guid userId)
{
    var order = await _repository.GetByIdAsync(orderId);
    
    if (order == null || order.Status != "Processed")
        return false;
    
    order.Status = "Approved";
    order.UpdatedDate = DateTime.Now;
    order.UpdatedBy = userId;
    
    await _repository.UpdateAsync(order);
    return true;
}
```

### 3. Order Cancellation
```csharp
public async Task<bool> CancelPurchaseOrderAsync(Guid orderId, string reason)
{
    if (string.IsNullOrWhiteSpace(reason))
        return false;
    
    var order = await _repository.GetByIdAsync(orderId);
    
    if (order == null || order.Status == "Cancelled")
        return false;
    
    // If already processed, reverse inventory changes
    if (order.Status == "Processed")
    {
        await ReverseInventoryChanges(order);
    }
    
    order.Status = "Cancelled";
    order.CancellationReason = reason;
    order.UpdatedDate = DateTime.Now;
    
    await _repository.UpdateAsync(order);
    return true;
}
```

## 📊 Dashboard Integration

### KPI Cards
- **Total Orders**: Count of all purchase orders
- **Pending Orders**: Orders awaiting processing
- **Processed Orders**: Orders ready for approval
- **Order Value**: Total value of active orders

### Charts & Analytics
- **Order Trends**: Monthly order volume and value
- **Supplier Performance**: Top suppliers by order volume
- **Status Distribution**: Pie chart of order statuses
- **Processing Time**: Average time from creation to approval

## 🎯 User Workflows

### 1. Creating a Purchase Order
1. Navigate to **Purchase Orders** → **Create New**
2. Select **Supplier** from dropdown
3. Add **Products** with quantities and unit prices
4. Review **Total Amount** calculation
5. Add **Notes** if needed
6. Click **Create Order**
7. Order created with **Pending** status

### 2. Processing Orders (Inventory Update)
1. Go to **Purchase Orders** list
2. Find **Pending** order
3. Click **Process** button
4. Confirm action in modal
5. System automatically:
   - Updates order status to **Processed**
   - Creates/updates **ProductBatch** records
   - Adjusts **inventory levels**
   - Shows success message

### 3. Approving Orders
1. Filter orders by **Processed** status
2. Review order details
3. Click **Approve** button
4. Order status changes to **Approved**
5. Order is now complete

### 4. Cancelling Orders
1. Select order to cancel
2. Click **Cancel** button
3. Enter **cancellation reason** in modal
4. Confirm cancellation
5. If order was processed, inventory is automatically reversed

## 🔍 Advanced Features

### 1. Smart Filtering
```html
<!-- Status Filter -->
<select id="statusFilter">
    <option value="">All Statuses</option>
    <option value="pending">Pending</option>
    <option value="processed">Processed</option>
    <option value="approved">Approved</option>
    <option value="cancelled">Cancelled</option>
</select>

<!-- Date Range Filter -->
<input type="date" id="fromDate" />
<input type="date" id="toDate" />

<!-- Supplier Filter -->
<select id="supplierFilter">
    <option value="">All Suppliers</option>
    <!-- Dynamic supplier list -->
</select>
```

### 2. Bulk Operations
- **Bulk Processing**: Process multiple pending orders
- **Bulk Approval**: Approve multiple processed orders
- **Export Selection**: Export filtered results
- **Print Reports**: Generate purchase reports

### 3. Audit Trail
- **Creation Tracking**: Who created the order and when
- **Status Changes**: Full history of status transitions
- **Modification Log**: All changes with timestamps
- **User Actions**: Track all user interactions

## 📈 Performance Optimizations

### Database Optimizations
```sql
-- Indexes for fast filtering
CREATE INDEX IX_PurchaseOrders_Status ON PurchaseOrders(Status);
CREATE INDEX IX_PurchaseOrders_OrderDate ON PurchaseOrders(OrderDate);
CREATE INDEX IX_PurchaseOrders_SupplierID ON PurchaseOrders(SupplierID);

-- Composite index for common queries
CREATE INDEX IX_PurchaseOrders_Status_OrderDate 
ON PurchaseOrders(Status, OrderDate);
```

### Query Optimizations
```csharp
// Efficient loading with related data
var orders = await _context.PurchaseOrders
    .Include(o => o.Supplier)
    .Include(o => o.Items)
        .ThenInclude(i => i.Product)
    .Where(o => o.Status == status)
    .OrderByDescending(o => o.OrderDate)
    .ToListAsync();

// Projection for list views (better performance)
var orderSummaries = await _context.PurchaseOrders
    .Select(o => new PurchaseOrderSummaryDTO
    {
        PurchaseOrderID = o.PurchaseOrderID,
        OrderNumber = o.OrderNumber,
        OrderDate = o.OrderDate,
        Status = o.Status,
        TotalAmount = o.TotalAmount,
        SupplierName = o.Supplier.SupplierName
    })
    .ToListAsync();
```

## 🚀 Future Enhancements

### Planned Features
- **Email Notifications**: Notify suppliers of new orders
- **Approval Workflow**: Multi-level approval process
- **Budget Controls**: Order approval based on budget limits
- **Recurring Orders**: Automatic reordering for regular supplies
- **Mobile App**: Mobile interface for order management
- **Barcode Integration**: Scan products during receiving
- **Supplier Portal**: Let suppliers view and respond to orders

### Integration Opportunities
- **Accounting Systems**: Sync with financial software
- **Supplier EDI**: Electronic data interchange with suppliers
- **Inventory Forecasting**: AI-powered demand prediction
- **Quality Control**: Track product quality metrics
- **Compliance Tracking**: Regulatory compliance monitoring

---

> 💡 **Best Practice**: Always process orders promptly to maintain accurate inventory levels. The system is designed to handle the complete procurement workflow while maintaining data integrity and providing excellent user experience.