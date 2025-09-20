# 💻 Development Setup Guide

Complete guide for setting up the development environment for the Pharmacy Management System.

## 🎯 Development Environment Overview

This guide helps developers set up a complete development environment for contributing to the Pharmacy Management System.

### What You'll Set Up
- Development tools and IDEs
- Database development environment
- Project structure understanding
- Debugging and testing setup
- Version control workflow

## 🛠️ Required Tools & Software

### Essential Development Tools

| Tool | Version | Purpose | Download Link |
|------|---------|---------|---------------|
| **.NET SDK** | 8.0+ | Core framework | [Download](https://dotnet.microsoft.com/download) |
| **Visual Studio 2022** | Latest | Primary IDE | [Download](https://visualstudio.microsoft.com/) |
| **SQL Server** | 2019+ | Database | [Download](https://www.microsoft.com/sql-server/) |
| **Git** | Latest | Version control | [Download](https://git-scm.com/) |
| **Node.js** | 18+ | Frontend tools | [Download](https://nodejs.org/) |

### Optional but Recommended

| Tool | Purpose | Download Link |
|------|---------|---------------|
| **VS Code** | Lightweight editing | [Download](https://code.visualstudio.com/) |
| **SQL Server Management Studio** | Database management | [Download](https://docs.microsoft.com/sql/ssms/) |
| **Postman** | API testing (future) | [Download](https://www.postman.com/) |
| **LINQPad** | Query testing | [Download](https://www.linqpad.net/) |

## 🚀 Initial Setup

### 1. Clone the Repository
```bash
# Clone the main repository
git clone https://github.com/nazzmullxd/Pharmacy-Management-System.git
cd Pharmacy-Management-System

# Create your development branch
git checkout -b feature/your-feature-name
```

### 2. Visual Studio Setup
1. **Open Visual Studio 2022**
2. **Open Project**: Choose "Open a project or solution"
3. **Select**: `Pharmacy Management System.sln`
4. **Restore NuGet Packages**: Right-click solution → Restore NuGet Packages

### 3. Configure Solution Properties
```
Solution 'Pharmacy Management System'
├── Business (Class Library)
├── Database (Class Library)
├── Web (ASP.NET Core Web App)
├── Desktop (WPF - Optional)
├── DbConnectionTest (Console - Testing)
└── DBTest (Console - Testing)
```

## 🗄️ Database Development Setup

### 1. SQL Server Configuration
```sql
-- Create development database
CREATE DATABASE PharmacyManagementSystem_Dev;

-- Verify connection
SELECT @@VERSION, @@SERVERNAME, DB_NAME();
```

### 2. Connection String Setup
Update `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=PharmacyManagementSystem_Dev;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### 3. Database Migration Setup
```bash
# Navigate to Database project
cd Database

# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Update database with latest migrations
dotnet ef database update --startup-project ../Web

# Verify migration success
dotnet ef migrations list --startup-project ../Web
```

## 🏗️ Project Structure Deep Dive

### Business Layer Structure
```
Business/
├── DTOs/                       # Data Transfer Objects
│   ├── AntibioticLogDTO.cs     # Antibiotic tracking
│   ├── AuditLogDTO.cs          # System audit logs
│   ├── BatchDTO.cs             # Product batches
│   ├── CustomerDTO.cs          # Customer data
│   ├── DashboardKPIDTO.cs      # Dashboard metrics
│   ├── ExpiryAlertDTO.cs       # Expiry notifications
│   ├── ProductBatchDTO.cs      # Product batch details
│   ├── ProductDTO.cs           # Product information
│   ├── PurchaseDTO.cs          # Purchase records
│   ├── PurchaseItemDTO.cs      # Purchase line items
│   ├── PurchaseOrderDTO.cs     # Purchase orders
│   ├── PurchaseOrderItemDTO.cs # Purchase order items
│   ├── ReportDTO.cs            # Report data
│   ├── SaleDTO.cs              # Sales transactions
│   ├── SaleItemDTO.cs          # Sale line items
│   ├── StockAdjustmentDTO.cs   # Stock adjustments
│   ├── SupplierDTO.cs          # Supplier information
│   ├── SupportTicketDTO.cs     # Support tickets
│   ├── TopProductDTO.cs        # Top selling products
│   └── UserDTO.cs              # User accounts
├── Interfaces/                 # Service Contracts
│   ├── ICustomerService.cs     # Customer operations
│   ├── IDashboardService.cs    # Dashboard data
│   ├── INotificationService.cs # Notifications
│   ├── IProductService.cs      # Product operations
│   ├── IPurchaseOrderService.cs# Purchase workflow
│   ├── IReportService.cs       # Report generation
│   ├── ISalesService.cs        # Sales operations
│   ├── IStockAdjustmentService.cs # Stock adjustments
│   ├── IStockService.cs        # Stock management
│   ├── ISupplierService.cs     # Supplier operations
│   ├── ISupportTicketService.cs# Support system
│   └── IUserService.cs         # User management
├── Services/                   # Business Logic
│   ├── CustomerService.cs      # Customer business logic
│   ├── DashboardService.cs     # Dashboard operations
│   ├── NotificationService.cs  # Notification handling
│   ├── ProductService.cs       # Product management
│   ├── PurchaseOrderService.cs # Purchase order workflow
│   ├── ReportService.cs        # Report generation
│   ├── SalesService.cs         # Sales processing
│   ├── StockAdjustmentService.cs # Stock operations
│   ├── StockService.cs         # Inventory management
│   ├── SupplierService.cs      # Supplier operations
│   ├── SupportTicketService.cs # Support ticket handling
│   └── UserService.cs          # User operations
└── ServiceRegistration.cs      # Dependency injection
```

### Database Layer Structure
```
Database/
├── Context/                    # EF Core Context
│   └── PharmacyManagementContext.cs
├── Model/                      # Entity Models
│   ├── AntibioticLog.cs        # Antibiotic tracking
│   ├── AuditLog.cs             # Audit trails
│   ├── Customer.cs             # Customer entity
│   ├── Product.cs              # Product entity
│   ├── ProductBatch.cs         # Product batches
│   ├── Purchase.cs             # Purchase records
│   ├── PurchaseItem.cs         # Purchase items
│   ├── Sale.cs                 # Sales transactions
│   ├── SaleItem.cs             # Sale items
│   ├── StockAdjustment.cs      # Stock adjustments
│   ├── Supplier.cs             # Supplier entity
│   ├── SupportTicket.cs        # Support tickets
│   └── User.cs                 # User accounts
├── Repositories/               # Data Access
│   ├── CustomerRepository.cs   # Customer data access
│   ├── ProductRepository.cs    # Product data access
│   ├── SalesRepository.cs      # Sales data access
│   ├── StockRepository.cs      # Stock data access
│   └── SupplierRepository.cs   # Supplier data access
├── Interfaces/                 # Repository Contracts
│   ├── ICustomerRepository.cs  # Customer contract
│   ├── IProductRepository.cs   # Product contract
│   ├── ISalesRepository.cs     # Sales contract
│   ├── IStockRepository.cs     # Stock contract
│   └── ISupplierRepository.cs  # Supplier contract
├── Migrations/                 # EF Core Migrations
│   └── [timestamp]_InitialCreate.cs
└── DesignTimeDbContextFactory.cs # Design-time factory
```

### Web Layer Structure
```
Web/
├── Pages/                      # Razor Pages
│   ├── Shared/                 # Shared components
│   │   ├── _Layout.cshtml      # Main layout
│   │   └── _ViewImports.cshtml # Global imports
│   ├── Dashboard/              # Dashboard pages
│   │   └── Index.cshtml        # Main dashboard
│   ├── Products/               # Product management
│   │   ├── Index.cshtml        # Product list
│   │   ├── Create.cshtml       # Add product
│   │   ├── Edit.cshtml         # Edit product
│   │   └── Details.cshtml      # Product details
│   ├── Sales/                  # Sales management
│   │   ├── Index.cshtml        # Sales list
│   │   ├── Create.cshtml       # New sale
│   │   ├── Invoice.cshtml      # Invoice view
│   │   └── Invoices.cshtml     # Invoice list
│   ├── Purchases/              # Purchase orders
│   │   ├── Index.cshtml        # Order list
│   │   ├── Create.cshtml       # New order
│   │   └── Details.cshtml      # Order details
│   ├── Customers/              # Customer management
│   │   ├── Index.cshtml        # Customer list
│   │   ├── Create.cshtml       # Add customer
│   │   ├── Edit.cshtml         # Edit customer
│   │   └── Details.cshtml      # Customer details
│   ├── Suppliers/              # Supplier management
│   │   ├── Index.cshtml        # Supplier list
│   │   ├── Create.cshtml       # Add supplier
│   │   └── Details.cshtml      # Supplier details
│   ├── Users/                  # User management
│   │   └── Index.cshtml        # User list
│   └── Reports/                # Reports & analytics
│       └── Index.cshtml        # Report dashboard
├── wwwroot/                    # Static files
│   ├── css/                    # Stylesheets
│   │   └── site.css            # Main styles
│   ├── js/                     # JavaScript
│   │   └── site.js             # Main scripts
│   ├── lib/                    # Third-party libraries
│   │   ├── bootstrap/          # Bootstrap framework
│   │   ├── jquery/             # jQuery library
│   │   └── font-awesome/       # Icon library
│   └── images/                 # Image assets
├── Properties/                 # Project properties
│   └── launchSettings.json     # Launch configuration
├── appsettings.json            # Production settings
├── appsettings.Development.json # Development settings
└── Program.cs                  # Application entry point
```

## 🔧 Development Workflow

### 1. Feature Development Process
```bash
# 1. Create feature branch
git checkout -b feature/your-feature-name

# 2. Make changes
# Edit code, add features, fix bugs

# 3. Test changes
dotnet build
dotnet test

# 4. Commit changes
git add .
git commit -m "Add: Your feature description"

# 5. Push to remote
git push origin feature/your-feature-name

# 6. Create Pull Request
# Use GitHub interface to create PR
```

### 2. Code Organization Standards
```csharp
// Service Implementation Example
namespace Business.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(
            ICustomerRepository repository,
            ILogger<CustomerService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync()
        {
            try
            {
                var customers = await _repository.GetAllAsync();
                return customers.Select(MapToDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers");
                throw;
            }
        }

        private static CustomerDTO MapToDTO(Customer customer)
        {
            return new CustomerDTO
            {
                CustomerID = customer.CustomerID,
                CustomerName = customer.CustomerName,
                // ... other properties
            };
        }
    }
}
```

## 🧪 Testing Setup

### 1. Unit Testing Configuration
```bash
# Add test project (if not exists)
dotnet new xunit -n Pharmacy.Tests
dotnet add Pharmacy.Tests package Microsoft.EntityFrameworkCore.InMemory
dotnet add Pharmacy.Tests package Moq

# Add project reference
dotnet add Pharmacy.Tests reference Business/Business.csproj
```

### 2. Test Example
```csharp
[Test]
public async Task GetAllCustomersAsync_ShouldReturnAllCustomers()
{
    // Arrange
    var mockRepository = new Mock<ICustomerRepository>();
    var customers = new List<Customer>
    {
        new Customer { CustomerID = Guid.NewGuid(), CustomerName = "Test Customer" }
    };
    mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(customers);
    
    var service = new CustomerService(mockRepository.Object, Mock.Of<ILogger<CustomerService>>());

    // Act
    var result = await service.GetAllCustomersAsync();

    // Assert
    Assert.Single(result);
    mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
}
```

## 🐛 Debugging Setup

### 1. Visual Studio Debugging
1. Set **Web** as startup project
2. Set breakpoints in code
3. Press **F5** to start debugging
4. Use **Debug** → **Windows** for advanced debugging tools

### 2. Database Debugging
```sql
-- Enable SQL logging in appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### 3. Browser Developer Tools
- **F12**: Open browser developer tools
- **Console**: View JavaScript errors and logs
- **Network**: Monitor API calls and responses
- **Application**: Check local storage and cookies

## 📊 Performance Monitoring

### 1. Application Insights (Optional)
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-key-here"
  }
}
```

### 2. Custom Logging
```csharp
public class CustomerService : ICustomerService
{
    private readonly ILogger<CustomerService> _logger;

    public async Task<CustomerDTO> CreateCustomerAsync(CustomerDTO customerDto)
    {
        _logger.LogInformation("Creating customer: {CustomerName}", customerDto.CustomerName);
        
        var stopwatch = Stopwatch.StartNew();
        try
        {
            // Business logic here
            var result = await _repository.CreateAsync(customer);
            
            stopwatch.Stop();
            _logger.LogInformation("Customer created successfully in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            
            return MapToDTO(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer: {CustomerName}", customerDto.CustomerName);
            throw;
        }
    }
}
```

## 🔄 Development Best Practices

### 1. Code Quality Standards
- **Consistent Naming**: Follow C# naming conventions
- **XML Documentation**: Document public APIs
- **Error Handling**: Comprehensive exception handling
- **Logging**: Log important operations and errors
- **Async/Await**: Use async patterns for database operations

### 2. Git Workflow
```bash
# Regular development cycle
git checkout master
git pull origin master
git checkout -b feature/new-feature
# Make changes
git add .
git commit -m "Add: Descriptive commit message"
git push origin feature/new-feature
# Create Pull Request
```

### 3. Database Development
- **Migrations**: Always use EF Core migrations
- **Seed Data**: Use data seeding for development data
- **Backup**: Regular database backups
- **Testing**: Test with realistic data volumes

## 🚀 Advanced Development Topics

### 1. Custom Middleware
```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
        await _next(context);
    }
}
```

### 2. Background Services
```csharp
public class InventoryMonitoringService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InventoryMonitoringService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var stockService = scope.ServiceProvider.GetRequiredService<IStockService>();
            
            await CheckLowStockItems(stockService);
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
```

### 3. API Development (Future)
```csharp
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetCustomers()
    {
        var customers = await _customerService.GetAllCustomersAsync();
        return Ok(customers);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDTO>> CreateCustomer(CustomerDTO customer)
    {
        var result = await _customerService.CreateCustomerAsync(customer);
        return CreatedAtAction(nameof(GetCustomer), new { id = result.CustomerID }, result);
    }
}
```

## 🔧 Troubleshooting Development Issues

### Common Problems and Solutions

#### Build Errors
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build
```

#### Database Issues
```bash
# Reset database
dotnet ef database drop --startup-project ../Web
dotnet ef database update --startup-project ../Web

# Check migrations
dotnet ef migrations list --startup-project ../Web
```

#### Performance Issues
- Profile database queries
- Monitor memory usage
- Check for N+1 query problems
- Optimize LINQ queries

---

> 💡 **Development Success**: Focus on writing clean, testable code that follows established patterns. The architecture supports rapid development while maintaining code quality and system reliability.

**Need Help?** Check the [System Architecture](../System-Architecture.md) guide or reach out to the development team for assistance.