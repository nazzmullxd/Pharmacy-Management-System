# 🏗️ System Architecture

The Pharmacy Management System follows **Clean Architecture** principles with clear separation of concerns and dependency inversion.

## 📐 Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────────────────┐│
│  │ Razor Pages │ │   wwwroot   │ │    Controllers/APIs     ││
│  │             │ │   (Static)  │ │    (Future)            ││
│  └─────────────┘ └─────────────┘ └─────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Business Logic Layer                     │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────────────────┐│
│  │  Services   │ │    DTOs     │ │      Interfaces        ││
│  │             │ │             │ │                        ││
│  └─────────────┘ └─────────────┘ └─────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Data Access Layer                        │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────────────────┐│
│  │Repositories │ │   Models    │ │      DbContext         ││
│  │             │ │  (Entities) │ │                        ││
│  └─────────────┘ └─────────────┘ └─────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                        Database                             │
│                     SQL Server                              │
└─────────────────────────────────────────────────────────────┘
```

## 🎯 Design Principles

### 1. Clean Architecture
- **Dependency Inversion**: High-level modules don't depend on low-level modules
- **Separation of Concerns**: Each layer has distinct responsibilities
- **Testability**: Easy to unit test business logic in isolation
- **Maintainability**: Changes in one layer don't affect others

### 2. Domain-Driven Design (DDD)
- **Entities**: Core business objects (Product, Sale, Customer, etc.)
- **Value Objects**: Immutable objects (Money, Address, etc.)
- **Aggregates**: Consistency boundaries (Order with OrderItems)
- **Services**: Business logic operations

### 3. Repository Pattern
- **Abstraction**: Hide data access complexity
- **Testing**: Easy to mock for unit tests
- **Flexibility**: Can switch data sources without changing business logic
- **Consistency**: Standardized data access patterns

## 📦 Layer Details

### Presentation Layer (Web Project)

**Responsibilities:**
- User interface rendering
- HTTP request/response handling
- Input validation and binding
- Authentication and authorization (future)

**Components:**
```
Web/
├── Pages/                  # Razor Pages
│   ├── Dashboard/         # Dashboard components
│   ├── Products/          # Product management
│   ├── Sales/             # Sales processing
│   ├── Purchases/         # Purchase orders
│   ├── Customers/         # Customer management
│   ├── Suppliers/         # Supplier management
│   ├── Users/             # User management
│   └── Reports/           # Analytics & reports
├── wwwroot/               # Static files
│   ├── css/              # Stylesheets
│   ├── js/               # JavaScript files
│   ├── lib/              # Third-party libraries
│   └── images/           # Images and icons
├── Program.cs             # Application bootstrap
└── appsettings.json       # Configuration
```

### Business Logic Layer (Business Project)

**Responsibilities:**
- Business rules and logic
- Data transformation (Entity ↔ DTO)
- Validation and error handling
- Workflow orchestration

**Components:**
```
Business/
├── Services/              # Service implementations
│   ├── CustomerService.cs      # Customer operations
│   ├── ProductService.cs       # Product & inventory
│   ├── SalesService.cs         # Sales processing
│   ├── PurchaseOrderService.cs # Purchase workflow
│   ├── DashboardService.cs     # Analytics & KPIs
│   ├── StockService.cs         # Inventory management
│   ├── SupplierService.cs      # Supplier operations
│   ├── UserService.cs          # User management
│   └── ReportService.cs        # Report generation
├── DTOs/                  # Data Transfer Objects
│   ├── ProductDTO.cs           # Product data
│   ├── SaleDTO.cs              # Sales data
│   ├── CustomerDTO.cs          # Customer data
│   ├── PurchaseOrderDTO.cs     # Purchase order data
│   └── [20+ other DTOs]        # Other business objects
├── Interfaces/            # Service contracts
│   ├── ICustomerService.cs     # Customer service contract
│   ├── IProductService.cs      # Product service contract
│   ├── ISalesService.cs        # Sales service contract
│   └── [12+ other interfaces]  # Other service contracts
└── ServiceRegistration.cs # DI container setup
```

### Data Access Layer (Database Project)

**Responsibilities:**
- Database context and configuration
- Entity definitions and relationships
- Repository implementations
- Database migrations

**Components:**
```
Database/
├── Context/               # EF Core context
│   └── PharmacyManagementContext.cs
├── Models/                # Entity models
│   ├── Product.cs              # Product entity
│   ├── Sale.cs                 # Sale entity
│   ├── Customer.cs             # Customer entity
│   ├── PurchaseOrder.cs        # Purchase order entity
│   ├── ProductBatch.cs         # Inventory batch
│   └── [15+ other models]      # Other entities
├── Repositories/          # Data access implementations
│   ├── ProductRepository.cs    # Product data access
│   ├── SalesRepository.cs      # Sales data access
│   ├── CustomerRepository.cs   # Customer data access
│   └── [repository classes]    # Other repositories
├── Interfaces/            # Repository contracts
│   ├── IProductRepository.cs   # Product repository contract
│   ├── ISalesRepository.cs     # Sales repository contract
│   └── [interface files]       # Other repository contracts
└── Migrations/            # Database migrations
    └── [migration files]       # EF Core migrations
```

## 🔄 Data Flow Architecture

### 1. Request Processing Flow
```
User Request → Razor Page → Service Layer → Repository → Database
                    ↓           ↓            ↓
               Page Model ← DTO ← Entity ← Database Result
```

### 2. Dependency Injection Flow
```
Program.cs
    ↓
ServiceRegistration.AddBusinessServices()
    ↓
Container registers:
    - ICustomerService → CustomerService
    - ICustomerRepository → CustomerRepository
    - DbContext → PharmacyManagementContext
```

### 3. Purchase Order Workflow
```
Create Order → Validate Business Rules → Save to Database
     ↓
Update Status → Process Inventory → Update Stock Levels
     ↓
Generate Reports → Send Notifications → Complete Workflow
```

## 🎯 Key Architectural Patterns

### 1. Service Layer Pattern
**Purpose**: Encapsulate business logic and provide a consistent API

**Implementation:**
```csharp
public interface ICustomerService
{
    Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync();
    Task<CustomerDTO> GetCustomerByIdAsync(Guid id);
    Task<CustomerDTO> CreateCustomerAsync(CustomerDTO customer);
    Task<bool> UpdateCustomerAsync(CustomerDTO customer);
    Task<bool> DeleteCustomerAsync(Guid id);
}

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    
    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }
    
    // Implementation with business logic
}
```

### 2. Repository Pattern
**Purpose**: Abstract data access and provide testable data layer

**Implementation:**
```csharp
public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer> GetByIdAsync(Guid id);
    Task<Customer> CreateAsync(Customer customer);
    Task<bool> UpdateAsync(Customer customer);
    Task<bool> DeleteAsync(Guid id);
}

public class CustomerRepository : ICustomerRepository
{
    private readonly PharmacyManagementContext _context;
    
    // Implementation with EF Core
}
```

### 3. DTO Pattern
**Purpose**: Transfer data between layers without exposing domain models

**Benefits:**
- **Security**: Don't expose internal entity structure
- **Versioning**: Can modify DTOs without changing entities
- **Performance**: Only transfer needed data
- **Validation**: Layer-specific validation rules

## 🔧 Configuration & Dependencies

### Dependency Injection Setup
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services to container
builder.Services.AddDbContext<PharmacyManagementContext>(options =>
    options.UseSqlServer(connectionString));

// Register business services
Business.ServiceRegistration.AddBusinessServices(builder.Services);

// Register repositories
Database.ServiceRegistration.AddRepositories(builder.Services);

var app = builder.Build();
```

### Service Registration
```csharp
// Business/ServiceRegistration.cs
public static class ServiceRegistration
{
    public static IServiceCollection AddBusinessServices(
        this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISalesService, SalesService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        // ... other services
        
        return services;
    }
}
```

## 📊 Performance Considerations

### 1. Database Optimization
- **Indexing**: Strategic indexes on frequently queried columns
- **Lazy Loading**: Disabled by default, explicit loading where needed
- **Query Optimization**: Efficient LINQ queries with projections
- **Connection Pooling**: EF Core connection pooling enabled

### 2. Caching Strategy
- **Memory Caching**: Frequently accessed reference data
- **Distributed Caching**: For multi-instance deployments (future)
- **Query Result Caching**: Cache expensive query results
- **Application-Level Caching**: Business logic caching

### 3. Async Programming
- **Async/Await**: All database operations are asynchronous
- **Non-blocking**: UI remains responsive during operations
- **Scalability**: Better resource utilization
- **Performance**: Improved throughput

## 🔍 Architecture Benefits

### 1. Maintainability
- **Clear Structure**: Easy to navigate and understand
- **Separation**: Changes in one layer don't affect others
- **Standards**: Consistent patterns across the application
- **Documentation**: Well-documented interfaces and services

### 2. Testability
- **Unit Testing**: Business logic easily testable in isolation
- **Mocking**: Interfaces allow easy mocking for tests
- **Integration Testing**: Clear boundaries for integration tests
- **Test Data**: Repository pattern allows test data injection

### 3. Scalability
- **Horizontal Scaling**: Stateless service layer
- **Database Scaling**: Repository pattern allows database optimization
- **Caching**: Multiple caching layers for performance
- **Load Balancing**: Stateless design supports load balancing

### 4. Flexibility
- **Database Independence**: Can switch databases with minimal changes
- **UI Technology**: Can add different UI technologies (API, mobile)
- **Third-party Integration**: Clean interfaces for external systems
- **Business Rule Changes**: Easy to modify business logic

## 🚀 Future Architecture Enhancements

### Planned Improvements
- **API Layer**: RESTful APIs for mobile and external integration
- **Microservices**: Split into domain-specific microservices
- **Event Sourcing**: Track all business events for audit and replay
- **CQRS**: Separate read and write models for better performance
- **Message Queues**: Asynchronous processing for heavy operations

### Technology Upgrades
- **Docker**: Containerization for easier deployment
- **Kubernetes**: Orchestration for cloud deployments
- **Redis**: Distributed caching and session storage
- **SignalR**: Real-time updates and notifications
- **GraphQL**: Flexible query API for front-end applications

---

> 💡 **Architecture Philosophy**: The system is designed for **long-term maintainability** and **business value delivery**. Every architectural decision prioritizes **code quality**, **developer productivity**, and **system reliability**.