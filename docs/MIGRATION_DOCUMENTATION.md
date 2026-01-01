# Pharmacy Management System - Razor Pages to MVC Migration Documentation

## Table of Contents
1. [Executive Summary](#executive-summary)
2. [Current System Architecture](#current-system-architecture)
3. [Target Architecture](#target-architecture)
4. [Role-Based Access Control (RBAC) Design](#role-based-access-control-rbac-design)
5. [Migration Modules](#migration-modules)
6. [Detailed Implementation Plan](#detailed-implementation-plan)
7. [Database Schema Changes](#database-schema-changes)
8. [Security Implementation](#security-implementation)
9. [File Mapping Reference](#file-mapping-reference)
10. [Testing Strategy](#testing-strategy)

---

## Executive Summary

This document outlines the comprehensive migration plan from **ASP.NET Core Razor Pages** to **ASP.NET Core MVC** architecture for the M/S Rabiul Pharmacy Management System. The migration includes implementing **Role-Based Access Control (RBAC)** with two primary roles:

| Role | Access Level | Description |
|------|--------------|-------------|
| **Admin** | Full Access | Complete system management including user management, settings, reports, and all CRUD operations |
| **User** | Limited Access | Basic operations like sales, customer management, and viewing products/stock |

### Migration Goals
- ✅ Convert all Razor Pages to MVC Controllers and Views
- ✅ Implement proper authentication using ASP.NET Core Identity (optional) or custom authentication
- ✅ Implement RBAC authorization
- ✅ Maintain existing UI/UX design
- ✅ Preserve all business logic (Business layer remains unchanged)
- ✅ Preserve all database operations (Database layer remains unchanged)

---

## Current System Architecture

### Project Structure
```
Pharmacy-Management-System/
├── Web/                    # Razor Pages Frontend (TO BE MIGRATED)
│   ├── Pages/
│   │   ├── Shared/_Layout.cshtml
│   │   ├── Dashboard.cshtml[.cs]
│   │   ├── UserLogin.cshtml[.cs]
│   │   ├── UserRegistration.cshtml[.cs]
│   │   ├── Customers/      # Index, Create, Edit, Details, Import
│   │   ├── Products/       # Index, Create, Edit, Details, Categories, New
│   │   ├── Sales/          # Index, Create, Details, New, Orders, Invoice, Invoices
│   │   ├── Stock/          # Items, Batches, Adjustments, Expiring
│   │   ├── Purchases/      # Index, Create, New, Orders
│   │   ├── Suppliers/      # Index, Create, Edit, Details
│   │   ├── Reports/        # Index, Sales, Stock, ProfitLoss, Antibiotic
│   │   ├── Users/          # Index
│   │   ├── Pharmacy/       # Info, Settings
│   │   └── Api/            # CustomersSearch
│   └── wwwroot/
├── MVC - WEB/              # MVC Frontend (TARGET)
├── Business/               # Business Logic Layer (UNCHANGED)
│   ├── DTO/
│   ├── Interfaces/
│   └── Services/
└── Database/               # Data Access Layer (UNCHANGED)
    ├── Model/
    ├── Interfaces/
    └── Repositories/
```

### Existing Authentication Mechanism
The current system uses **Session-based authentication**:
- Session key: `auth` (value: "1" when authenticated)
- Session key: `userEmail` (stores logged-in user email)
- Session key: `role` (stores user role: "Admin" or "Employee")

### Existing Services
| Service | Interface | Description |
|---------|-----------|-------------|
| UserService | IUserService | User CRUD, authentication, password management |
| ProductService | IProductService | Product CRUD, search, categories |
| SalesService | ISalesService | Sales CRUD, processing, payment status |
| CustomerService | ICustomerService | Customer CRUD, search |
| SupplierService | ISupplierService | Supplier CRUD, status management |
| StockService | IStockService | Stock/batch management, expiry alerts |
| PurchaseOrderService | IPurchaseOrderService | Purchase orders, approvals |
| ReportService | IReportService | Sales, inventory, profit reports |
| DashboardService | IDashboardService | KPIs, top products, alerts |
| SupportTicketService | ISupportTicketService | Support ticket management |
| NotificationService | INotificationService | System notifications |
| StockAdjustmentService | IStockAdjustmentService | Stock adjustments |

---

## Target Architecture

### MVC Project Structure
```
MVC - WEB/
├── Controllers/
│   ├── AccountController.cs          # Login, Logout, Register
│   ├── DashboardController.cs        # Dashboard views
│   ├── HomeController.cs             # Landing pages
│   ├── CustomersController.cs        # Customer management
│   ├── ProductsController.cs         # Product management
│   ├── SalesController.cs            # Sales management
│   ├── StockController.cs            # Stock management
│   ├── PurchasesController.cs        # Purchase management
│   ├── SuppliersController.cs        # Supplier management
│   ├── ReportsController.cs          # Reports generation
│   ├── UsersController.cs            # User management (Admin only)
│   ├── PharmacyController.cs         # Pharmacy info & settings
│   ├── SupportController.cs          # Support tickets
│   ├── ProfileController.cs          # User profile
│   ├── SettingsController.cs         # User settings
│   └── Api/
│       └── CustomersApiController.cs # API endpoints
├── Models/
│   ├── ViewModels/                   # View-specific models
│   │   ├── LoginViewModel.cs
│   │   ├── RegisterViewModel.cs
│   │   ├── DashboardViewModel.cs
│   │   └── ...
│   └── ErrorViewModel.cs
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml            # Main layout
│   │   ├── _LoginLayout.cshtml       # Auth pages layout
│   │   ├── _ValidationScriptsPartial.cshtml
│   │   └── Error.cshtml
│   ├── Account/
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   ├── Dashboard/
│   │   └── Index.cshtml
│   ├── Customers/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   ├── Details.cshtml
│   │   └── Import.cshtml
│   ├── Products/
│   ├── Sales/
│   ├── Stock/
│   ├── Purchases/
│   ├── Suppliers/
│   ├── Reports/
│   ├── Users/
│   ├── Pharmacy/
│   └── Support/
├── Filters/                          # Custom authorization filters
│   ├── AuthenticationFilter.cs
│   └── RoleAuthorizationFilter.cs
├── Middleware/
│   └── SessionAuthenticationMiddleware.cs
├── wwwroot/                          # Static files (copy from Web)
│   ├── css/
│   ├── js/
│   └── lib/
├── appsettings.json
└── Program.cs
```

---

## Role-Based Access Control (RBAC) Design

### Role Definitions

#### Admin Role
**Full system access including:**
- ✅ Dashboard (full KPIs and analytics)
- ✅ User Management (Create, Edit, Delete, View all users)
- ✅ Product Management (Create, Edit, Delete, Categories)
- ✅ Customer Management (Full CRUD + Import)
- ✅ Sales Management (Create, Edit, Delete, View invoices)
- ✅ Stock Management (Full CRUD + Adjustments)
- ✅ Purchase Management (Create, Edit, Delete, Approve)
- ✅ Supplier Management (Full CRUD)
- ✅ Reports (All reports including Profit/Loss)
- ✅ Pharmacy Settings (Edit pharmacy information)
- ✅ Support Tickets (Manage all tickets)
- ✅ System Settings

#### User (Employee) Role
**Limited operational access:**
- ✅ Dashboard (basic KPIs)
- ❌ User Management (No access)
- ✅ Product Management (View only)
- ✅ Customer Management (View, Create)
- ✅ Sales Management (Create, View own sales)
- ✅ Stock Management (View only)
- ❌ Purchase Management (No access)
- ✅ Supplier Management (View only)
- ✅ Reports (Sales reports only)
- ❌ Pharmacy Settings (No access)
- ✅ Support Tickets (Create, View own tickets)
- ✅ Profile (Own profile only)

### Permission Matrix

| Module | Action | Admin | User |
|--------|--------|-------|------|
| **Dashboard** | View | ✅ Full | ✅ Basic |
| **Users** | List | ✅ | ❌ |
| **Users** | Create/Edit/Delete | ✅ | ❌ |
| **Products** | List | ✅ | ✅ |
| **Products** | Create/Edit/Delete | ✅ | ❌ |
| **Customers** | List | ✅ | ✅ |
| **Customers** | Create | ✅ | ✅ |
| **Customers** | Edit/Delete | ✅ | ❌ |
| **Customers** | Import | ✅ | ❌ |
| **Sales** | List All | ✅ | ❌ |
| **Sales** | List Own | ✅ | ✅ |
| **Sales** | Create | ✅ | ✅ |
| **Sales** | Edit/Delete | ✅ | ❌ |
| **Stock** | List | ✅ | ✅ |
| **Stock** | Adjustments | ✅ | ❌ |
| **Purchases** | All Operations | ✅ | ❌ |
| **Suppliers** | List | ✅ | ✅ |
| **Suppliers** | Create/Edit/Delete | ✅ | ❌ |
| **Reports** | All Reports | ✅ | ❌ |
| **Reports** | Sales Reports | ✅ | ✅ |
| **Pharmacy** | View Info | ✅ | ✅ |
| **Pharmacy** | Settings | ✅ | ❌ |
| **Support** | Create Ticket | ✅ | ✅ |
| **Support** | Manage All | ✅ | ❌ |
| **Profile** | Own Profile | ✅ | ✅ |
| **Settings** | System Settings | ✅ | ❌ |

---

## Migration Modules

The migration is divided into **12 modules** to be implemented sequentially:

### Module 1: Project Setup & Authentication (Priority: HIGH)
**Estimated Time: 4-6 hours**

| Task | Description | Files |
|------|-------------|-------|
| 1.1 | Update MVC project dependencies | `MVC - WEB.csproj` |
| 1.2 | Configure Program.cs with services | `Program.cs` |
| 1.3 | Create Authentication Filter | `Filters/AuthenticationFilter.cs` |
| 1.4 | Create Role Authorization Filter | `Filters/RoleAuthorizationFilter.cs` |
| 1.5 | Create Session Middleware | `Middleware/SessionAuthMiddleware.cs` |
| 1.6 | Create Account Controller | `Controllers/AccountController.cs` |
| 1.7 | Create Login/Register ViewModels | `Models/ViewModels/Account/` |
| 1.8 | Create Login/Register Views | `Views/Account/` |
| 1.9 | Create Auth Layout | `Views/Shared/_LoginLayout.cshtml` |

### Module 2: Layout & Shared Components (Priority: HIGH)
**Estimated Time: 2-3 hours**

| Task | Description | Files |
|------|-------------|-------|
| 2.1 | Migrate Main Layout | `Views/Shared/_Layout.cshtml` |
| 2.2 | Migrate CSS files | `wwwroot/css/` |
| 2.3 | Migrate JS files | `wwwroot/js/` |
| 2.4 | Configure ViewImports | `Views/_ViewImports.cshtml` |
| 2.5 | Configure ViewStart | `Views/_ViewStart.cshtml` |
| 2.6 | Create Navigation partial with RBAC | `Views/Shared/_Navigation.cshtml` |
| 2.7 | Create Error views | `Views/Shared/Error.cshtml` |

### Module 3: Dashboard (Priority: HIGH)
**Estimated Time: 3-4 hours**

| Task | Description | Files |
|------|-------------|-------|
| 3.1 | Create Dashboard Controller | `Controllers/DashboardController.cs` |
| 3.2 | Create Dashboard ViewModel | `Models/ViewModels/DashboardViewModel.cs` |
| 3.3 | Migrate Dashboard View | `Views/Dashboard/Index.cshtml` |
| 3.4 | Implement role-based KPI filtering | Controller logic |

### Module 4: User Management (Priority: HIGH)
**Estimated Time: 4-5 hours**

| Task | Description | Files |
|------|-------------|-------|
| 4.1 | Create Users Controller | `Controllers/UsersController.cs` |
| 4.2 | Create User ViewModels | `Models/ViewModels/Users/` |
| 4.3 | Create Users Index View | `Views/Users/Index.cshtml` |
| 4.4 | Create User Create View | `Views/Users/Create.cshtml` |
| 4.5 | Create User Edit View | `Views/Users/Edit.cshtml` |
| 4.6 | Create User Details View | `Views/Users/Details.cshtml` |
| 4.7 | Apply Admin-only authorization | Controller attributes |

### Module 5: Product Management (Priority: HIGH)
**Estimated Time: 4-5 hours**

| Task | Description | Files |
|------|-------------|-------|
| 5.1 | Create Products Controller | `Controllers/ProductsController.cs` |
| 5.2 | Create Product ViewModels | `Models/ViewModels/Products/` |
| 5.3 | Migrate Products Index View | `Views/Products/Index.cshtml` |
| 5.4 | Migrate Products Create View | `Views/Products/Create.cshtml` |
| 5.5 | Migrate Products Edit View | `Views/Products/Edit.cshtml` |
| 5.6 | Migrate Products Details View | `Views/Products/Details.cshtml` |
| 5.7 | Migrate Categories View | `Views/Products/Categories.cshtml` |
| 5.8 | Apply role-based access | Controller logic |

### Module 6: Customer Management (Priority: MEDIUM)
**Estimated Time: 3-4 hours**

| Task | Description | Files |
|------|-------------|-------|
| 6.1 | Create Customers Controller | `Controllers/CustomersController.cs` |
| 6.2 | Create Customer ViewModels | `Models/ViewModels/Customers/` |
| 6.3 | Migrate Customer Views | `Views/Customers/` |
| 6.4 | Create Customers API Controller | `Controllers/Api/CustomersApiController.cs` |
| 6.5 | Apply role-based access (Create for all, Edit/Delete for Admin) | Controller logic |

### Module 7: Sales Management (Priority: HIGH)
**Estimated Time: 5-6 hours**

| Task | Description | Files |
|------|-------------|-------|
| 7.1 | Create Sales Controller | `Controllers/SalesController.cs` |
| 7.2 | Create Sales ViewModels | `Models/ViewModels/Sales/` |
| 7.3 | Migrate Sales Index View | `Views/Sales/Index.cshtml` |
| 7.4 | Migrate New Sale View | `Views/Sales/New.cshtml` |
| 7.5 | Migrate Sales Orders View | `Views/Sales/Orders.cshtml` |
| 7.6 | Migrate Invoice View | `Views/Sales/Invoice.cshtml` |
| 7.7 | Migrate Invoices List View | `Views/Sales/Invoices.cshtml` |
| 7.8 | Migrate Sales Details View | `Views/Sales/Details.cshtml` |
| 7.9 | Implement user-based sale filtering | Controller logic |

### Module 8: Stock Management (Priority: MEDIUM)
**Estimated Time: 4-5 hours**

| Task | Description | Files |
|------|-------------|-------|
| 8.1 | Create Stock Controller | `Controllers/StockController.cs` |
| 8.2 | Create Stock ViewModels | `Models/ViewModels/Stock/` |
| 8.3 | Migrate Stock Items View | `Views/Stock/Items.cshtml` |
| 8.4 | Migrate Batches View | `Views/Stock/Batches.cshtml` |
| 8.5 | Migrate Adjustments View | `Views/Stock/Adjustments.cshtml` |
| 8.6 | Migrate Expiring View | `Views/Stock/Expiring.cshtml` |
| 8.7 | Apply Admin-only for adjustments | Controller logic |

### Module 9: Purchase Management (Priority: MEDIUM)
**Estimated Time: 3-4 hours**

| Task | Description | Files |
|------|-------------|-------|
| 9.1 | Create Purchases Controller | `Controllers/PurchasesController.cs` |
| 9.2 | Create Purchase ViewModels | `Models/ViewModels/Purchases/` |
| 9.3 | Migrate Purchase Views | `Views/Purchases/` |
| 9.4 | Apply Admin-only authorization | Controller attributes |

### Module 10: Supplier Management (Priority: MEDIUM)
**Estimated Time: 2-3 hours**

| Task | Description | Files |
|------|-------------|-------|
| 10.1 | Create Suppliers Controller | `Controllers/SuppliersController.cs` |
| 10.2 | Create Supplier ViewModels | `Models/ViewModels/Suppliers/` |
| 10.3 | Migrate Supplier Views | `Views/Suppliers/` |
| 10.4 | Apply role-based access | Controller logic |

### Module 11: Reports (Priority: MEDIUM)
**Estimated Time: 3-4 hours**

| Task | Description | Files |
|------|-------------|-------|
| 11.1 | Create Reports Controller | `Controllers/ReportsController.cs` |
| 11.2 | Create Report ViewModels | `Models/ViewModels/Reports/` |
| 11.3 | Migrate Reports Index View | `Views/Reports/Index.cshtml` |
| 11.4 | Migrate Sales Report View | `Views/Reports/Sales.cshtml` |
| 11.5 | Migrate Stock Report View | `Views/Reports/Stock.cshtml` |
| 11.6 | Migrate Profit/Loss View | `Views/Reports/ProfitLoss.cshtml` |
| 11.7 | Migrate Antibiotic View | `Views/Reports/Antibiotic.cshtml` |
| 11.8 | Apply role-based access (Admin: All, User: Sales only) | Controller logic |

### Module 12: Support, Profile & Settings (Priority: LOW)
**Estimated Time: 2-3 hours**

| Task | Description | Files |
|------|-------------|-------|
| 12.1 | Create Support Controller | `Controllers/SupportController.cs` |
| 12.2 | Create Profile Controller | `Controllers/ProfileController.cs` |
| 12.3 | Create Settings Controller | `Controllers/SettingsController.cs` |
| 12.4 | Create Pharmacy Controller | `Controllers/PharmacyController.cs` |
| 12.5 | Migrate corresponding Views | `Views/Support/`, `Views/Profile/`, etc. |
| 12.6 | Apply role-based access | Controller logic |

---

## Detailed Implementation Plan

### Phase 1: Foundation (Modules 1-2)
**Duration: 1-2 days**

```
Day 1:
├── Module 1: Project Setup & Authentication
│   ├── Configure Program.cs
│   ├── Create Authentication system
│   └── Create Account Controller & Views

Day 2:
├── Module 2: Layout & Shared Components
│   ├── Migrate Layout
│   ├── Setup static files
│   └── Configure navigation with RBAC
```

### Phase 2: Core Features (Modules 3-7)
**Duration: 3-4 days**

```
Day 3:
├── Module 3: Dashboard
└── Module 4: User Management

Day 4:
├── Module 5: Product Management

Day 5:
├── Module 6: Customer Management
└── Module 7: Sales Management (Part 1)

Day 6:
├── Module 7: Sales Management (Part 2)
```

### Phase 3: Secondary Features (Modules 8-12)
**Duration: 2-3 days**

```
Day 7:
├── Module 8: Stock Management
└── Module 9: Purchase Management

Day 8:
├── Module 10: Supplier Management
└── Module 11: Reports

Day 9:
├── Module 12: Support, Profile & Settings
└── Final Integration Testing
```

---

## Database Schema Changes

### No Changes Required
The existing database schema supports RBAC with the `UserInfo.Role` field:

```csharp
public class UserInfo
{
    public Guid UserID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }  // "Admin" or "User"
    public DateTime LastLoginDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
```

### Recommended: Add Audit Fields (Optional)
For better tracking, consider adding:
- `CreatedBy` (Guid) - Foreign key to UserInfo
- `UpdatedBy` (Guid) - Foreign key to UserInfo

---

## Security Implementation

### 1. Custom Authorization Attributes

```csharp
// AdminOnlyAttribute.cs
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminOnlyAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var role = context.HttpContext.Session.GetString("role");
        if (role != "Admin")
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
        }
    }
}

// AuthenticatedAttribute.cs
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthenticatedAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var auth = context.HttpContext.Session.GetString("auth");
        if (auth != "1")
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
        }
    }
}
```

### 2. Controller Usage Example

```csharp
[Authenticated]  // All actions require authentication
public class UsersController : Controller
{
    [AdminOnly]  // Only Admin can access
    public IActionResult Index() { }
    
    [AdminOnly]
    public IActionResult Create() { }
    
    [AdminOnly]
    public IActionResult Edit(Guid id) { }
    
    [AdminOnly]
    public IActionResult Delete(Guid id) { }
}
```

### 3. View-Level Authorization

```cshtml
@{
    var role = Context.Session.GetString("role");
    var isAdmin = role == "Admin";
}

@if (isAdmin)
{
    <a asp-controller="Users" asp-action="Create" class="btn btn-primary">
        <i class="fas fa-user-plus"></i> Add User
    </a>
}
```

### 4. Navigation Role-Based Rendering

```cshtml
<!-- In _Layout.cshtml -->
@{
    var role = Context.Session.GetString("role");
    var isAdmin = role == "Admin";
}

<ul class="nav-menu">
    <li><a href="/Dashboard">Dashboard</a></li>
    
    @if (isAdmin)
    {
        <li><a href="/Users">User Management</a></li>
        <li><a href="/Purchases">Purchases</a></li>
    }
    
    <!-- Accessible to all authenticated users -->
    <li><a href="/Products">Products</a></li>
    <li><a href="/Sales">Sales</a></li>
</ul>
```

---

## File Mapping Reference

### Razor Pages to MVC Mapping

| Razor Page | MVC Controller | MVC Action | MVC View |
|------------|----------------|------------|----------|
| `UserLogin.cshtml` | `AccountController` | `Login` | `Views/Account/Login.cshtml` |
| `UserRegistration.cshtml` | `AccountController` | `Register` | `Views/Account/Register.cshtml` |
| `Logout.cshtml` | `AccountController` | `Logout` | Redirect |
| `Dashboard.cshtml` | `DashboardController` | `Index` | `Views/Dashboard/Index.cshtml` |
| `Products/Index.cshtml` | `ProductsController` | `Index` | `Views/Products/Index.cshtml` |
| `Products/Create.cshtml` | `ProductsController` | `Create` | `Views/Products/Create.cshtml` |
| `Products/Edit.cshtml` | `ProductsController` | `Edit` | `Views/Products/Edit.cshtml` |
| `Products/Details.cshtml` | `ProductsController` | `Details` | `Views/Products/Details.cshtml` |
| `Products/Categories.cshtml` | `ProductsController` | `Categories` | `Views/Products/Categories.cshtml` |
| `Customers/Index.cshtml` | `CustomersController` | `Index` | `Views/Customers/Index.cshtml` |
| `Customers/Create.cshtml` | `CustomersController` | `Create` | `Views/Customers/Create.cshtml` |
| `Customers/Edit.cshtml` | `CustomersController` | `Edit` | `Views/Customers/Edit.cshtml` |
| `Customers/Details.cshtml` | `CustomersController` | `Details` | `Views/Customers/Details.cshtml` |
| `Customers/Import.cshtml` | `CustomersController` | `Import` | `Views/Customers/Import.cshtml` |
| `Sales/Index.cshtml` | `SalesController` | `Index` | `Views/Sales/Index.cshtml` |
| `Sales/New.cshtml` | `SalesController` | `New` | `Views/Sales/New.cshtml` |
| `Sales/Create.cshtml` | `SalesController` | `Create` | `Views/Sales/Create.cshtml` |
| `Sales/Orders.cshtml` | `SalesController` | `Orders` | `Views/Sales/Orders.cshtml` |
| `Sales/Invoice.cshtml` | `SalesController` | `Invoice` | `Views/Sales/Invoice.cshtml` |
| `Sales/Invoices.cshtml` | `SalesController` | `Invoices` | `Views/Sales/Invoices.cshtml` |
| `Sales/Details.cshtml` | `SalesController` | `Details` | `Views/Sales/Details.cshtml` |
| `Stock/Items.cshtml` | `StockController` | `Items` | `Views/Stock/Items.cshtml` |
| `Stock/Batches.cshtml` | `StockController` | `Batches` | `Views/Stock/Batches.cshtml` |
| `Stock/Adjustments.cshtml` | `StockController` | `Adjustments` | `Views/Stock/Adjustments.cshtml` |
| `Stock/Expiring.cshtml` | `StockController` | `Expiring` | `Views/Stock/Expiring.cshtml` |
| `Purchases/Index.cshtml` | `PurchasesController` | `Index` | `Views/Purchases/Index.cshtml` |
| `Purchases/New.cshtml` | `PurchasesController` | `New` | `Views/Purchases/New.cshtml` |
| `Purchases/Create.cshtml` | `PurchasesController` | `Create` | `Views/Purchases/Create.cshtml` |
| `Purchases/Orders.cshtml` | `PurchasesController` | `Orders` | `Views/Purchases/Orders.cshtml` |
| `Suppliers/Index.cshtml` | `SuppliersController` | `Index` | `Views/Suppliers/Index.cshtml` |
| `Suppliers/Create.cshtml` | `SuppliersController` | `Create` | `Views/Suppliers/Create.cshtml` |
| `Suppliers/Edit.cshtml` | `SuppliersController` | `Edit` | `Views/Suppliers/Edit.cshtml` |
| `Suppliers/Details.cshtml` | `SuppliersController` | `Details` | `Views/Suppliers/Details.cshtml` |
| `Reports/Index.cshtml` | `ReportsController` | `Index` | `Views/Reports/Index.cshtml` |
| `Reports/Sales.cshtml` | `ReportsController` | `Sales` | `Views/Reports/Sales.cshtml` |
| `Reports/Stock.cshtml` | `ReportsController` | `Stock` | `Views/Reports/Stock.cshtml` |
| `Reports/ProfitLoss.cshtml` | `ReportsController` | `ProfitLoss` | `Views/Reports/ProfitLoss.cshtml` |
| `Reports/Antibiotic.cshtml` | `ReportsController` | `Antibiotic` | `Views/Reports/Antibiotic.cshtml` |
| `Users/Index.cshtml` | `UsersController` | `Index` | `Views/Users/Index.cshtml` |
| `Pharmacy/Info.cshtml` | `PharmacyController` | `Info` | `Views/Pharmacy/Info.cshtml` |
| `Pharmacy/Settings.cshtml` | `PharmacyController` | `Settings` | `Views/Pharmacy/Settings.cshtml` |
| `Support.cshtml` | `SupportController` | `Index` | `Views/Support/Index.cshtml` |
| `Profile.cshtml` | `ProfileController` | `Index` | `Views/Profile/Index.cshtml` |
| `Settings.cshtml` | `SettingsController` | `Index` | `Views/Settings/Index.cshtml` |

---

## Testing Strategy

### 1. Unit Tests
- Test authorization filters
- Test role-based access logic
- Test controller actions with mock services

### 2. Integration Tests
- Test authentication flow
- Test role-based redirects
- Test CRUD operations per role

### 3. Manual Testing Checklist

#### Admin Role Testing
- [ ] Login as Admin
- [ ] Access Dashboard (full view)
- [ ] Access User Management
- [ ] Create/Edit/Delete User
- [ ] Access all Products actions
- [ ] Access all Customers actions
- [ ] Access all Sales actions
- [ ] Access all Stock actions
- [ ] Access all Purchases actions
- [ ] Access all Suppliers actions
- [ ] Access all Reports
- [ ] Access Pharmacy Settings

#### User Role Testing
- [ ] Login as User
- [ ] Access Dashboard (limited view)
- [ ] Verify User Management is hidden/blocked
- [ ] View Products only
- [ ] Create Customer
- [ ] Verify Edit/Delete Customer blocked
- [ ] Create Sale
- [ ] View own Sales
- [ ] View Stock (read-only)
- [ ] Verify Purchases is hidden/blocked
- [ ] View Suppliers only
- [ ] Access Sales Reports only
- [ ] View Pharmacy Info (not settings)

---

## Appendix A: Key Code Templates

### Base Controller Template

```csharp
using Microsoft.AspNetCore.Mvc;
using MVC_WEB.Filters;

namespace MVC_WEB.Controllers
{
    [Authenticated]
    public class BaseController : Controller
    {
        protected string? CurrentUserEmail => HttpContext.Session.GetString("userEmail");
        protected string CurrentRole => HttpContext.Session.GetString("role") ?? "User";
        protected bool IsAdmin => CurrentRole == "Admin";
        
        protected Guid GetCurrentUserId()
        {
            var userId = HttpContext.Session.GetString("userId");
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }
}
```

### ViewModel Template

```csharp
namespace MVC_WEB.Models.ViewModels
{
    public class BaseViewModel
    {
        public string UserEmail { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public bool IsAdmin => Role == "Admin";
    }
}
```

---

## Appendix B: URL Route Mapping

### Route Configuration

```csharp
// Program.cs
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller}/{action}/{id?}");
```

### Expected URLs

| Feature | URL Pattern | HTTP Method |
|---------|-------------|-------------|
| Login | `/Account/Login` | GET, POST |
| Register | `/Account/Register` | GET, POST |
| Logout | `/Account/Logout` | POST |
| Dashboard | `/Dashboard` or `/` | GET |
| Products List | `/Products` | GET |
| Product Create | `/Products/Create` | GET, POST |
| Product Edit | `/Products/Edit/{id}` | GET, POST |
| Product Delete | `/Products/Delete/{id}` | POST |
| Customers List | `/Customers` | GET |
| Sales List | `/Sales` | GET |
| New Sale | `/Sales/New` | GET, POST |
| Reports | `/Reports` | GET |
| Users List | `/Users` | GET |

---

## Document Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-01-01 | Migration Team | Initial documentation |

---

## Next Steps

1. **Review this documentation** and confirm RBAC permissions matrix
2. **Start Module 1** - Project Setup & Authentication
3. **Progress through modules** in order
4. **Test each module** before moving to the next
5. **Final integration testing** after all modules complete

---

**Note:** This migration preserves the existing Business and Database layers completely. Only the presentation layer (Web → MVC) is being changed.
