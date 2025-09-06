# Pharmacy Management System - Business Logic Overview

## Overview
This document provides a comprehensive overview of the business logic layer for the Pharmacy Management System. The business layer implements the core business rules, validation, and data transformation logic for the application.

## Architecture

### Layer Structure
```
Business/
├── DTO/                    # Data Transfer Objects
├── Interfaces/             # Service interfaces
├── Services/              # Service implementations
├── ServiceRegistration.cs # Dependency injection setup
└── BUSINESS_LOGIC_OVERVIEW.md
```

## Data Transfer Objects (DTOs)

### Core DTOs
- **UserDTO**: User information with computed properties
- **ProductDTO**: Product information with stock calculations
- **SaleDTO**: Sales transaction with items and customer info
- **SaleItemDTO**: Individual items within a sale
- **CustomerDTO**: Customer information
- **SupplierDTO**: Supplier information
- **ProductBatchDTO**: Product batch with expiry tracking
- **PurchaseDTO**: Purchase transactions
- **PurchaseItemDTO**: Individual items within a purchase

### Reporting DTOs
- **ReportDTO**: Comprehensive report data
- **TopProductDTO**: Top-selling products with rankings
- **ExpiryAlertDTO**: Product expiry alerts with severity levels
- **AuditLogDTO**: System audit trail
- **AntibioticLogDTO**: Antibiotic sales tracking

## Service Interfaces

### IUserService
- User authentication and authorization
- User CRUD operations
- Password management
- Role-based access control

### IProductService
- Product catalog management
- Product validation and business rules
- Stock level calculations
- Barcode uniqueness validation

### ISalesService
- Sales transaction processing
- Sales reporting and analytics
- Top-selling products analysis
- Payment status management

### IStockService
- Inventory management
- Batch tracking and expiry monitoring
- Stock adjustments and movements
- Low stock alerts

### ISupplierService
- Supplier management
- Contact information validation
- Supplier status management
- Business relationship tracking

### IReportService
- Comprehensive reporting system
- Sales and inventory reports
- Financial analytics
- Audit trail reporting

### INotificationService
- Alert and notification system
- Expiry date monitoring
- Low stock notifications
- Customizable notification preferences

## Service Implementations

### UserService
- **Authentication**: Secure password hashing using SHA256
- **Validation**: Email format and uniqueness validation
- **Authorization**: Role-based access control
- **Security**: Password change with current password verification

### ProductService
- **Validation**: Comprehensive product data validation
- **Business Rules**: Price validation, category requirements
- **Stock Integration**: Integration with batch management
- **Status Management**: Product activation/deactivation

### SalesService
- **Transaction Processing**: Complete sale workflow
- **Data Integrity**: Validation of sale items and totals
- **Reporting**: Sales analytics and top products
- **Payment Tracking**: Payment status management

### StockService
- **Batch Management**: Product batch lifecycle
- **Expiry Monitoring**: Automated expiry date tracking
- **Stock Adjustments**: Inventory movement tracking
- **Alert System**: Low stock and expiry alerts

### SupplierService
- **Contact Management**: Supplier contact validation
- **Status Tracking**: Active/inactive supplier management
- **Business Rules**: Email format validation
- **Relationship Management**: Supplier-product relationships

### ReportService
- **Analytics**: Comprehensive business analytics
- **Date Range Reporting**: Flexible date-based reporting
- **Financial Reports**: Revenue and profit calculations
- **Inventory Reports**: Stock and expiry reporting

### NotificationService
- **Alert System**: Configurable notification system
- **Expiry Alerts**: Automated expiry date monitoring
- **Stock Alerts**: Low stock level notifications
- **Preference Management**: User-configurable notifications

## Key Features

### Business Rules
1. **Product Validation**: All products must have valid names, categories, and pricing
2. **Stock Management**: FIFO (First In, First Out) batch management
3. **Expiry Tracking**: Automatic monitoring of product expiry dates
4. **User Security**: Secure password hashing and authentication
5. **Data Integrity**: Comprehensive validation across all entities

### Validation Rules
- **Required Fields**: All essential fields are validated
- **Data Types**: Proper data type validation
- **Business Logic**: Custom business rule validation
- **Relationships**: Foreign key and relationship validation

### Error Handling
- **Exception Management**: Comprehensive exception handling
- **Validation Errors**: Detailed validation error messages
- **Graceful Degradation**: Fallback mechanisms for failures
- **Logging**: Audit trail for all operations

## Integration Points

### Database Layer
- Repository pattern for data access
- Entity-to-DTO mapping
- Transaction management
- Data persistence

### Presentation Layer
- DTO-based data transfer
- Service-based business logic
- Dependency injection
- Clean separation of concerns

## Usage Examples

### User Management
```csharp
// Create a new user
var userDto = new UserDTO { /* user data */ };
var createdUser = await userService.CreateUserAsync(userDto, "password123");

// Authenticate user
var isAuthenticated = await userService.AuthenticateUserAsync("user@example.com", "password123");
```

### Product Management
```csharp
// Create a product
var productDto = new ProductDTO { /* product data */ };
var createdProduct = await productService.CreateProductAsync(productDto);

// Get low stock products
var lowStockProducts = await productService.GetLowStockProductsAsync(threshold: 5);
```

### Sales Processing
```csharp
// Process a sale
var saleDto = new SaleDTO 
{
    CustomerID = customerId,
    UserID = userId,
    SaleItems = saleItems,
    TotalAmount = totalAmount
};
var processedSale = await salesService.ProcessSaleAsync(saleDto);
```

### Reporting
```csharp
// Generate sales report
var report = await reportService.GenerateSalesReportAsync(startDate, endDate);

// Get top selling products
var topProducts = await reportService.GetTopSellingProductsReportAsync(count: 10);
```

## Configuration

### Service Registration
```csharp
// In Program.cs or Startup.cs
services.AddBusinessServices();
```

### Dependency Injection
All services are registered with the DI container and can be injected into controllers or other services.

## Security Considerations

1. **Password Security**: SHA256 hashing for password storage
2. **Input Validation**: Comprehensive input validation
3. **Authorization**: Role-based access control
4. **Audit Trail**: Complete audit logging
5. **Data Protection**: Secure data handling

## Performance Considerations

1. **Async Operations**: All operations are asynchronous
2. **Efficient Queries**: Optimized database queries
3. **Caching**: Strategic caching where appropriate
4. **Batch Operations**: Bulk operations for efficiency

## Future Enhancements

1. **Caching Layer**: Redis or in-memory caching
2. **Event Sourcing**: Event-driven architecture
3. **Microservices**: Service decomposition
4. **API Versioning**: Version management
5. **Advanced Analytics**: Machine learning integration

## Testing

The business layer is designed to be easily testable with:
- Interface-based design for mocking
- Dependency injection for test doubles
- Clear separation of concerns
- Comprehensive validation testing

## Conclusion

The business logic layer provides a robust, scalable, and maintainable foundation for the Pharmacy Management System. It implements comprehensive business rules, validation, and data transformation while maintaining clean architecture principles and separation of concerns.
