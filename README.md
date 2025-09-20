# 🏥 Pharmacy Management System

A comprehensive, enterprise-grade pharmacy management system built with ASP.NET Core Razor Pages. Features complete business logic, advanced inventory management, and modern responsive UI/UX design.

## ✨ Key Features Overview

| Module | Features | Status |
|--------|----------|--------|
| 📊 **Dashboard** | Real-time KPIs, Sales analytics, Stock monitoring | ✅ Complete |
| 💊 **Products** | Catalog management, Batch tracking, Expiry alerts | ✅ Complete |
| 🛒 **Sales** | Transaction processing, Invoice generation, A4 printing | ✅ Complete |
| 📦 **Purchase Orders** | Status management, Inventory integration, Approval workflow | ✅ Complete |
| 👥 **Customers** | CRM, Contact management, Sales history | ✅ Complete |
| 🏭 **Suppliers** | Vendor management, Contact tracking, Purchase history | ✅ Complete |
| 👤 **Users** | Role-based access, User management, Activity tracking | ✅ Complete |
| 📈 **Reports** | Comprehensive analytics, Export capabilities, Print reports | ✅ Complete |

## 🚀 Latest Updates (v2.0)

### 🔄 Purchase Order Management
- **Complete Status Workflow**: Pending → Processed → Approved → Cancelled
- **Automatic Inventory Integration**: Stock levels update when orders are processed
- **Professional UI Controls**: Context-sensitive action buttons with validation
- **Cancel Order Modal**: Reason requirement with professional validation

### 🎯 Enhanced Business Logic
- **ProcessPurchaseOrderAsync**: Handles status transitions and inventory updates
- **ProductBatch Integration**: Automatic creation/updates when processing orders
- **Inventory Synchronization**: Real-time stock level adjustments
- **Error Handling**: Comprehensive exception handling and user feedback

### 🎨 UI/UX Improvements
- **Professional Design**: Modern, clean interface across all modules
- **Enhanced Forms**: Validation, progress tracking, and user guidance
- **A4 Print Optimization**: Proper invoice formatting for professional printing
- **Responsive Tables**: Advanced filtering, sorting, and search capabilities

## 🏗️ System Architecture

```
📁 Pharmacy-Management-System/
├── 🔧 Business/                    # Business Logic Layer
│   ├── 📄 DTOs/                   # Data Transfer Objects (20+ DTOs)
│   ├── 🔌 Interfaces/             # Service Contracts (12+ interfaces)
│   ├── ⚙️ Services/               # Business Logic Implementation
│   │   ├── CustomerService.cs     # Customer management
│   │   ├── ProductService.cs      # Product catalog & inventory
│   │   ├── SalesService.cs        # Sales processing
│   │   ├── PurchaseOrderService.cs # Purchase order workflow
│   │   ├── DashboardService.cs    # Analytics & KPIs
│   │   └── StockService.cs        # Inventory management
│   └── ServiceRegistration.cs     # Dependency injection setup
├── 🗄️ Database/                   # Data Access Layer
│   ├── 🔗 Context/                # Entity Framework DbContext
│   ├── 📋 Models/                 # Entity Models (15+ entities)
│   ├── 🔄 Migrations/             # Database migrations
│   ├── 🔌 Interfaces/             # Repository contracts
│   └── 📚 Repositories/           # Data access implementation
├── 🌐 Web/                        # Presentation Layer
│   ├── 📑 Pages/                  # Razor Pages
│   │   ├── Dashboard/             # KPI dashboard
│   │   ├── Products/              # Product management
│   │   ├── Sales/                 # Sales & invoicing
│   │   ├── Purchases/             # Purchase order management
│   │   ├── Customers/             # Customer management
│   │   ├── Suppliers/             # Supplier management
│   │   ├── Users/                 # User management
│   │   └── Reports/               # Analytics & reports
│   ├── 🎨 wwwroot/                # Static assets
│   └── Program.cs                 # Application bootstrap
└── 📝 Documentation/              # Project documentation
```

## 🛠️ Technology Stack

| Layer | Technology | Purpose |
|-------|------------|---------|
| **Backend** | ASP.NET Core 8.0, C# 12 | Server-side logic & APIs |
| **Frontend** | Razor Pages, HTML5, CSS3, JavaScript | User interface |
| **Database** | SQL Server, Entity Framework Core | Data persistence |
| **UI Framework** | Bootstrap 5.3, Font Awesome 6.4 | Responsive design |
| **Charts** | Chart.js | Data visualization |
| **Architecture** | Clean Architecture, Repository Pattern | Code organization |
| **DI Container** | Microsoft.Extensions.DependencyInjection | Dependency management |

## 🎯 Core Business Modules

### 📊 Dashboard & Analytics
- **Real-time KPIs**: Sales, revenue, stock value, customer metrics
- **Interactive Charts**: Sales trends, top products, revenue analytics
- **Quick Actions**: Direct navigation to key business functions
- **Alert System**: Low stock warnings, expiry notifications

### 💊 Product Management
- **Complete Catalog**: Product information, categories, pricing
- **Batch Tracking**: Expiry dates, supplier information, lot numbers
- **Inventory Control**: Stock levels, reorder points, adjustments
- **Price Management**: Unit, retail, wholesale pricing structures

### 🛒 Sales Management
- **Transaction Processing**: Point-of-sale functionality
- **Invoice Generation**: Professional invoicing with A4 print optimization
- **Payment Tracking**: Multiple payment methods and status tracking
- **Customer Integration**: Link sales to customer records

### 📦 Purchase Order Workflow
- **Status Management**: Complete workflow from creation to completion
- **Inventory Integration**: Automatic stock updates when orders are processed
- **Supplier Management**: Link orders to supplier records
- **Approval Process**: Multi-stage approval with validation

### 👥 Customer Relationship Management
- **Customer Database**: Complete contact and demographic information
- **Sales History**: Track customer purchase patterns
- **Professional UI**: Enhanced forms with validation and user guidance

### 🏭 Supplier Management
- **Vendor Database**: Comprehensive supplier information
- **Contact Management**: Multiple contact methods and personnel
- **Purchase History**: Track orders and supplier performance

## 🚀 Quick Start Guide

### Prerequisites
- **.NET 8.0 SDK** or later
- **SQL Server** (LocalDB, Express, or Full)
- **Visual Studio 2022** or **VS Code** with C# extension

### Installation Steps

1. **Clone the Repository**
   ```bash
   git clone https://github.com/nazzmullxd/Pharmacy-Management-System.git
   cd Pharmacy-Management-System
   ```

2. **Database Setup**
   ```bash
   cd Database
   dotnet ef database update --startup-project ../Web
   ```

3. **Install Dependencies**
   ```bash
   dotnet restore
   ```

4. **Configure Connection String**
   ```json
   // Web/appsettings.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=PharmacyManagementSystem;Trusted_Connection=true;"
     }
   }
   ```

5. **Run the Application**
   ```bash
   cd Web
   dotnet run
   ```

6. **Access the System**
   - Navigate to: `https://localhost:5067`
   - The dashboard will load with sample data

## 💡 Usage Examples

### Creating a Purchase Order
```csharp
// Business logic handles complete workflow
var order = new PurchaseOrderDTO
{
    SupplierID = supplierId,
    OrderDate = DateTime.Now,
    Status = "Pending",
    Items = purchaseItems
};

await _purchaseOrderService.CreatePurchaseOrderAsync(order);
```

### Processing an Order (Inventory Integration)
```csharp
// Automatically updates inventory when processing
var result = await _purchaseOrderService.ProcessPurchaseOrderAsync(orderId, userId);
// This creates/updates ProductBatch records and adjusts stock levels
```

## 📈 Business Intelligence Features

### Dashboard KPIs
- **Today's Sales**: Real-time sales amount and transaction count
- **Monthly Revenue**: Current month performance vs targets
- **Stock Value**: Total inventory valuation
- **Pending Orders**: Orders requiring attention
- **Expiring Products**: Items needing immediate attention

### Advanced Reports
- **Sales Analytics**: Daily, weekly, monthly sales reports
- **Inventory Reports**: Stock levels, expiry tracking, low stock alerts
- **Financial Reports**: Profit/loss, revenue analysis, expense tracking
- **Regulatory Reports**: Antibiotic usage tracking, compliance monitoring

## 🔧 Configuration & Customization

### Database Configuration
The system supports multiple SQL Server configurations:
- **LocalDB**: For development (default)
- **SQL Server Express**: For small deployments
- **SQL Server**: For production environments

### UI Customization
- **Bootstrap Themes**: Easy theme switching
- **Brand Colors**: Configurable color schemes
- **Logo Integration**: Custom branding support
- **Responsive Breakpoints**: Mobile-first design

## 🏆 Key Achievements

- **✅ Complete CRUD Operations**: All business entities fully implemented
- **✅ Advanced Business Logic**: Purchase order workflow with inventory integration
- **✅ Professional UI/UX**: Modern, responsive design across all modules
- **✅ Data Integrity**: Proper foreign key handling and validation
- **✅ Real-time Updates**: Live dashboard with current business metrics
- **✅ Print Optimization**: A4-formatted invoices and reports
- **✅ Error Handling**: Comprehensive exception handling and user feedback

## � Recent Enhancements

### Version 2.0 Features
- **Purchase Order Status Management**: Complete workflow implementation
- **Inventory Integration**: Automatic stock updates when processing orders
- **Enhanced UI Controls**: Professional forms with validation and progress tracking
- **Print Optimization**: A4 invoice formatting for professional printing
- **Advanced Filtering**: Improved search and filter capabilities across all modules

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. **Commit** your changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to the branch (`git push origin feature/AmazingFeature`)
5. **Open** a Pull Request

### Development Guidelines
- Follow C# coding conventions
- Include unit tests for new features
- Update documentation for significant changes
- Ensure responsive design for UI changes

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Development Team

| Developer | Role | GitHub |
|-----------|------|--------|
| **Nazmul Huda** | Lead Developer | [@nazzmullxd](https://github.com/nazzmullxd) |
| **Zahidul Islam** | Backend Developer | - |
| **Iftekhar Ahmed** | Frontend Developer | - |

## 🎯 Project Status

| Component | Status | Coverage |
|-----------|--------|----------|
| Core Business Logic | ✅ Complete | 100% |
| Database Schema | ✅ Complete | 100% |
| User Interface | ✅ Complete | 100% |
| Purchase Workflow | ✅ Complete | 100% |
| Inventory Management | ✅ Complete | 100% |
| Reports & Analytics | ✅ Complete | 95% |
| Unit Tests | 🚧 In Progress | 60% |
| Documentation | ✅ Complete | 95% |

## 🙏 Acknowledgments

- **ASP.NET Core Team**: For the excellent framework
- **Bootstrap Team**: For the responsive UI framework
- **Community Contributors**: For valuable feedback and suggestions
- **Real-world Pharmacies**: For inspiring the feature requirements

## 🔗 Useful Links

- **Live Demo**: [Coming Soon]
- **Documentation**: [Project Wiki](https://github.com/nazzmullxd/Pharmacy-Management-System/wiki)
- **Issue Tracker**: [GitHub Issues](https://github.com/nazzmullxd/Pharmacy-Management-System/issues)
- **Releases**: [GitHub Releases](https://github.com/nazzmullxd/Pharmacy-Management-System/releases)

---

> 💡 **Note**: This is a comprehensive pharmacy management solution designed for real-world use. The system includes all essential pharmacy operations with a modern, user-friendly interface and robust business logic.
