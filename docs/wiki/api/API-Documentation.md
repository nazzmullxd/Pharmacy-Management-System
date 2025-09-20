# 🔌 API Documentation

Comprehensive API documentation for the Pharmacy Management System (Future Implementation).

## 🎯 API Overview

The Pharmacy Management System will provide RESTful APIs for integration with external systems, mobile applications, and third-party services.

> **Current Status**: The system currently uses Razor Pages for web UI. API endpoints will be added in future versions to support mobile apps and integrations.

### API Goals
- **Mobile App Support**: Enable mobile pharmacy management apps
- **Third-Party Integration**: Connect with POS systems, accounting software
- **Automation**: Support automated inventory management systems
- **Reporting**: Provide data access for business intelligence tools
- **B2B Integration**: Connect with supplier and customer systems

## 🚀 Planned API Architecture

### Authentication & Authorization
```csharp
// JWT Token-based authentication
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class BaseApiController : ControllerBase
{
    protected string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    protected string UserRole => User.FindFirst(ClaimTypes.Role)?.Value;
}
```

### Response Format Standards
```json
{
  "success": true,
  "data": {
    // Response data
  },
  "message": "Operation completed successfully",
  "timestamp": "2024-01-15T10:30:00Z",
  "pagination": {
    "page": 1,
    "pageSize": 50,
    "totalPages": 10,
    "totalItems": 500
  }
}
```

### Error Response Format
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input data",
    "details": [
      {
        "field": "CustomerName",
        "message": "Customer name is required"
      }
    ]
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## 📊 Planned API Endpoints

### 1. Authentication & User Management

#### Authentication
```http
POST /api/v1/auth/login
POST /api/v1/auth/logout
POST /api/v1/auth/refresh
GET  /api/v1/auth/profile
PUT  /api/v1/auth/profile
```

#### Users
```http
GET    /api/v1/users              # List users (Admin only)
POST   /api/v1/users              # Create user (Admin only)
GET    /api/v1/users/{id}         # Get user details
PUT    /api/v1/users/{id}         # Update user
DELETE /api/v1/users/{id}         # Delete user (Admin only)
PUT    /api/v1/users/{id}/password # Change password
```

**Example: Login Request**
```json
POST /api/v1/auth/login
{
  "username": "pharmacist1",
  "password": "SecurePassword123"
}
```

**Example: Login Response**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "expiresIn": 3600,
    "user": {
      "id": "uuid-here",
      "username": "pharmacist1",
      "firstName": "John",
      "lastName": "Doe",
      "role": "Pharmacist",
      "email": "john.doe@pharmacy.com"
    }
  }
}
```

### 2. Products & Inventory Management

#### Products
```http
GET    /api/v1/products           # List products
POST   /api/v1/products           # Create product
GET    /api/v1/products/{id}      # Get product details
PUT    /api/v1/products/{id}      # Update product
DELETE /api/v1/products/{id}      # Delete product
GET    /api/v1/products/search    # Search products
```

**Example: Get Products**
```json
GET /api/v1/products?page=1&pageSize=50&category=Antibiotics&search=amoxicillin

{
  "success": true,
  "data": [
    {
      "productId": "uuid-here",
      "productName": "Amoxicillin 500mg",
      "category": "Antibiotics",
      "manufacturer": "PharmaCorp",
      "unitPrice": 25.50,
      "currentStock": 150,
      "reorderLevel": 50,
      "expiryDate": "2025-12-31",
      "batchNumber": "BATCH001",
      "description": "Broad-spectrum antibiotic",
      "isControlled": true,
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-15T10:30:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 50,
    "totalPages": 5,
    "totalItems": 250
  }
}
```

#### Product Batches
```http
GET    /api/v1/products/{id}/batches     # Get product batches
POST   /api/v1/products/{id}/batches     # Add new batch
PUT    /api/v1/products/batches/{id}     # Update batch
GET    /api/v1/products/batches/expiring # Get expiring batches
```

#### Stock Management
```http
GET    /api/v1/stock                     # Current stock levels
GET    /api/v1/stock/low                 # Low stock items
POST   /api/v1/stock/adjustment          # Stock adjustment
GET    /api/v1/stock/movements           # Stock movement history
```

### 3. Sales Management

#### Sales
```http
GET    /api/v1/sales              # List sales
POST   /api/v1/sales              # Create sale
GET    /api/v1/sales/{id}         # Get sale details
PUT    /api/v1/sales/{id}         # Update sale
DELETE /api/v1/sales/{id}         # Delete sale
GET    /api/v1/sales/{id}/invoice # Generate invoice
```

**Example: Create Sale**
```json
POST /api/v1/sales
{
  "customerId": "customer-uuid",
  "saleDate": "2024-01-15T14:30:00Z",
  "items": [
    {
      "productId": "product-uuid",
      "quantity": 2,
      "unitPrice": 25.50,
      "discount": 0
    },
    {
      "productId": "product-uuid-2",
      "quantity": 1,
      "unitPrice": 45.00,
      "discount": 5.00
    }
  ],
  "paymentMethod": "Cash",
  "notes": "Customer prescription #12345"
}
```

#### Sale Reports
```http
GET    /api/v1/sales/reports/daily       # Daily sales report
GET    /api/v1/sales/reports/monthly     # Monthly sales report
GET    /api/v1/sales/reports/top-products # Top selling products
```

### 4. Purchase Orders

#### Purchase Orders
```http
GET    /api/v1/purchase-orders           # List purchase orders
POST   /api/v1/purchase-orders           # Create purchase order
GET    /api/v1/purchase-orders/{id}      # Get order details
PUT    /api/v1/purchase-orders/{id}      # Update order
DELETE /api/v1/purchase-orders/{id}      # Delete order
POST   /api/v1/purchase-orders/{id}/process # Process order
POST   /api/v1/purchase-orders/{id}/approve # Approve order
POST   /api/v1/purchase-orders/{id}/cancel  # Cancel order
```

**Example: Purchase Order Status Update**
```json
POST /api/v1/purchase-orders/uuid-here/process
{
  "notes": "All items received and verified",
  "receivedBy": "John Doe",
  "receivedDate": "2024-01-15T16:00:00Z",
  "items": [
    {
      "purchaseOrderItemId": "item-uuid",
      "receivedQuantity": 100,
      "batchNumber": "BATCH002",
      "expiryDate": "2025-12-31",
      "notes": "Good condition"
    }
  ]
}
```

### 5. Customer Management

#### Customers
```http
GET    /api/v1/customers          # List customers
POST   /api/v1/customers          # Create customer
GET    /api/v1/customers/{id}     # Get customer details
PUT    /api/v1/customers/{id}     # Update customer
DELETE /api/v1/customers/{id}     # Delete customer
GET    /api/v1/customers/search   # Search customers
GET    /api/v1/customers/{id}/sales # Customer purchase history
```

### 6. Supplier Management

#### Suppliers
```http
GET    /api/v1/suppliers          # List suppliers
POST   /api/v1/suppliers          # Create supplier
GET    /api/v1/suppliers/{id}     # Get supplier details
PUT    /api/v1/suppliers/{id}     # Update supplier
DELETE /api/v1/suppliers/{id}     # Delete supplier
GET    /api/v1/suppliers/{id}/products # Supplier products
GET    /api/v1/suppliers/{id}/orders   # Supplier order history
```

### 7. Dashboard & Analytics

#### Dashboard
```http
GET    /api/v1/dashboard/kpi              # Key performance indicators
GET    /api/v1/dashboard/sales-summary    # Sales summary
GET    /api/v1/dashboard/stock-alerts     # Stock alerts
GET    /api/v1/dashboard/expiry-alerts    # Expiry alerts
GET    /api/v1/dashboard/recent-activity  # Recent activities
```

**Example: Dashboard KPIs**
```json
GET /api/v1/dashboard/kpi

{
  "success": true,
  "data": {
    "todaySales": {
      "totalSales": 15420.50,
      "totalTransactions": 87,
      "averageTransaction": 177.25
    },
    "inventory": {
      "totalProducts": 1250,
      "lowStockItems": 23,
      "expiringItems": 8,
      "totalValue": 487500.00
    },
    "customers": {
      "totalCustomers": 2340,
      "newThisMonth": 45,
      "repeatCustomers": 1890
    },
    "trends": {
      "salesGrowth": 12.5,
      "inventoryTurnover": 4.2,
      "customerRetention": 87.3
    }
  }
}
```

### 8. Reports & Analytics

#### Reports
```http
GET    /api/v1/reports/sales              # Sales reports
GET    /api/v1/reports/inventory          # Inventory reports
GET    /api/v1/reports/financial          # Financial reports
GET    /api/v1/reports/customer           # Customer reports
GET    /api/v1/reports/supplier           # Supplier reports
POST   /api/v1/reports/custom             # Custom reports
```

### 9. Notifications

#### Notifications
```http
GET    /api/v1/notifications              # List notifications
POST   /api/v1/notifications              # Create notification
PUT    /api/v1/notifications/{id}/read    # Mark as read
DELETE /api/v1/notifications/{id}         # Delete notification
GET    /api/v1/notifications/unread       # Get unread count
```

## 🔒 API Security Implementation

### JWT Authentication
```csharp
// JWT Configuration
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
        };
    });
```

### Role-Based Authorization
```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<ActionResult> DeleteProduct(Guid id)
{
    // Only admins can delete products
}

[Authorize(Roles = "Admin,Manager")]
[HttpPost("approve/{id}")]
public async Task<ActionResult> ApprovePurchaseOrder(Guid id)
{
    // Only admins and managers can approve orders
}
```

### Rate Limiting
```csharp
// Rate limiting configuration
services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", rateLimiterOptions =>
    {
        rateLimiterOptions.Window = TimeSpan.FromMinutes(1);
        rateLimiterOptions.PermitLimit = 100;
    });
});
```

### Input Validation
```csharp
[HttpPost]
public async Task<ActionResult<ProductDTO>> CreateProduct([FromBody] CreateProductRequest request)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(new ApiResponse<object>
        {
            Success = false,
            Error = new ApiError
            {
                Code = "VALIDATION_ERROR",
                Message = "Invalid input data",
                Details = ModelState.SelectMany(x => x.Value.Errors)
                    .Select(x => new ValidationError
                    {
                        Field = x.PropertyName,
                        Message = x.ErrorMessage
                    }).ToList()
            }
        });
    }

    // Process valid request
}
```

## 📱 Mobile App Integration

### Mobile-Specific Endpoints
```http
GET    /api/v1/mobile/dashboard           # Mobile dashboard
GET    /api/v1/mobile/products/barcode/{code} # Barcode scanning
POST   /api/v1/mobile/sales/quick         # Quick sale entry
GET    /api/v1/mobile/notifications/push  # Push notifications
POST   /api/v1/mobile/sync                # Data synchronization
```

### Offline Support
```json
// Sync request for offline support
POST /api/v1/mobile/sync
{
  "lastSyncTimestamp": "2024-01-15T08:00:00Z",
  "pendingTransactions": [
    {
      "tempId": "temp-sale-1",
      "type": "sale",
      "data": {
        // Sale data
      },
      "timestamp": "2024-01-15T10:30:00Z"
    }
  ]
}
```

## 🔧 API Development Guidelines

### Controller Structure
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProductsController : BaseApiController
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDTO>>>> GetProducts(
        [FromQuery] ProductSearchRequest request)
    {
        try
        {
            var products = await _productService.SearchProductsAsync(request);
            return Ok(new ApiResponse<IEnumerable<ProductDTO>>
            {
                Success = true,
                Data = products.Items,
                Pagination = new PaginationInfo
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = products.TotalPages,
                    TotalItems = products.TotalItems
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Error = new ApiError
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while processing your request"
                }
            });
        }
    }
}
```

### API Models
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public PaginationInfo Pagination { get; set; }
    public ApiError Error { get; set; }
}

public class ApiError
{
    public string Code { get; set; }
    public string Message { get; set; }
    public List<ValidationError> Details { get; set; }
}

public class ValidationError
{
    public string Field { get; set; }
    public string Message { get; set; }
}

public class PaginationInfo
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
}
```

## 📚 API Documentation Tools

### Swagger/OpenAPI Configuration
```csharp
// Swagger configuration
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pharmacy Management System API",
        Version = "v1",
        Description = "RESTful API for Pharmacy Management System",
        Contact = new OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@pharmacy-system.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
```

### API Testing
```csharp
// Integration test example
[Test]
public async Task GetProducts_ShouldReturnProductList()
{
    // Arrange
    var client = _factory.CreateClient();
    var token = await GetAuthTokenAsync(client);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    // Act
    var response = await client.GetAsync("/api/v1/products");

    // Assert
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();
    var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<ProductDTO>>>(content);
    
    Assert.True(apiResponse.Success);
    Assert.NotNull(apiResponse.Data);
}
```

## 🚀 Implementation Roadmap

### Phase 1: Core APIs (Month 1-2)
- [ ] Authentication & Authorization
- [ ] Products & Inventory APIs
- [ ] Basic Sales APIs
- [ ] Customer Management APIs

### Phase 2: Advanced Features (Month 3-4)
- [ ] Purchase Order APIs
- [ ] Dashboard & Analytics APIs
- [ ] Report Generation APIs
- [ ] Notification APIs

### Phase 3: Mobile & Integration (Month 5-6)
- [ ] Mobile-specific endpoints
- [ ] Third-party integration APIs
- [ ] Advanced reporting APIs
- [ ] Real-time notifications

### Phase 4: Advanced Features (Month 7+)
- [ ] Advanced analytics APIs
- [ ] B2B integration APIs
- [ ] Webhook support
- [ ] GraphQL endpoints (optional)

---

> 💡 **API Future**: The API layer will transform the Pharmacy Management System into a comprehensive platform supporting mobile apps, integrations, and advanced automation capabilities.

**Need More Info?** Check the [Development Setup](../development/Development-Setup.md) guide for API development environment setup.