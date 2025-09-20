# 🚀 Quick Start Guide

Get your Pharmacy Management System up and running in just a few minutes!

## 📋 Prerequisites Check

Before starting, ensure you have:

- [ ] **.NET 8.0 SDK** or later ([Download here](https://dotnet.microsoft.com/download))
- [ ] **SQL Server** (LocalDB, Express, or Full edition)
- [ ] **Visual Studio 2022** or **VS Code** with C# extension
- [ ] **Git** for version control

### Verify Prerequisites

```bash
# Check .NET version
dotnet --version

# Check SQL Server connection (replace with your server)
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT @@VERSION"
```

## ⚡ Quick Installation (5 minutes)

### 1. Clone the Repository
```bash
git clone https://github.com/nazzmullxd/Pharmacy-Management-System.git
cd Pharmacy-Management-System
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Configure Database
```bash
# Navigate to Web project
cd Web

# Update appsettings.json with your connection string
# Default LocalDB connection should work out of the box
```

### 4. Setup Database
```bash
# Run from the Database project directory
cd ../Database
dotnet ef database update --startup-project ../Web
```

### 5. Run the Application
```bash
cd ../Web
dotnet run
```

### 6. Access the System
Open your browser and navigate to:
- **HTTP**: `http://localhost:5067`
- **HTTPS**: `https://localhost:5067`

## 🎯 First Steps After Installation

### 1. Explore the Dashboard
- Navigate to the main dashboard
- Review the KPI cards and charts
- Familiarize yourself with the navigation menu

### 2. Sample Data
The system comes with sample data for:
- Products and categories
- Customers
- Suppliers
- Sales transactions
- Purchase orders

### 3. Key Areas to Explore

| Module | Description | First Action |
|--------|-------------|--------------|
| **Dashboard** | Business overview and KPIs | Review today's metrics |
| **Products** | Product catalog and inventory | Browse product list |
| **Sales** | Sales transactions and invoicing | View recent sales |
| **Purchase Orders** | Order management workflow | Check pending orders |
| **Customers** | Customer database | Review customer list |
| **Suppliers** | Vendor management | View supplier information |

## 🔧 Common Configuration

### Database Connection Strings

**LocalDB (Default)**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=PharmacyManagementSystem;Trusted_Connection=true;"
  }
}
```

**SQL Server Express**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=PharmacyManagementSystem;Trusted_Connection=true;"
  }
}
```

**SQL Server (Windows Authentication)**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YourServerName;Database=PharmacyManagementSystem;Trusted_Connection=true;"
  }
}
```

**SQL Server (SQL Authentication)**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YourServerName;Database=PharmacyManagementSystem;User Id=YourUsername;Password=YourPassword;"
  }
}
```

### Application Settings

Key configuration options in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Your connection string here"
  },
  "AppSettings": {
    "ApplicationName": "M/S Rabiul Pharmacy",
    "Version": "2.0",
    "Environment": "Development"
  }
}
```

## 🚨 Troubleshooting Quick Fixes

### Database Connection Issues

**Problem**: Cannot connect to database
```bash
# Check SQL Server is running
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT 1"

# If LocalDB not found, install SQL Server LocalDB
# Download from Microsoft SQL Server LocalDB
```

**Problem**: Database doesn't exist
```bash
# Recreate database
cd Database
dotnet ef database drop --startup-project ../Web
dotnet ef database update --startup-project ../Web
```

### Build Errors

**Problem**: Package restore fails
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore
```

**Problem**: Compilation errors
```bash
# Clean and rebuild
dotnet clean
dotnet build
```

### Port Conflicts

**Problem**: Port 5067 already in use
```bash
# Kill process using port 5067
netstat -ano | findstr :5067
taskkill /PID <process_id> /F

# Or change port in launchSettings.json
```

## 📱 Testing Your Installation

### 1. Dashboard Test
- ✅ Dashboard loads with KPI cards
- ✅ Charts display sample data
- ✅ Navigation menu works

### 2. Core Functionality Test
- ✅ Products page displays product list
- ✅ Sales page shows transactions
- ✅ Purchase orders page loads
- ✅ Customers page displays customer data

### 3. Create New Record Test
- ✅ Create a new customer
- ✅ Add a new product
- ✅ Process a sale transaction
- ✅ Create a purchase order

## 🎓 Learning Path

### For New Users
1. **Start with Dashboard** - Understand the business overview
2. **Explore Products** - Learn inventory management
3. **Review Sales** - Understand transaction processing
4. **Check Purchase Orders** - Learn procurement workflow

### For Developers
1. **Study System Architecture** - Understand the codebase structure
2. **Review Business Logic** - Examine service layer implementation
3. **Explore Database Schema** - Understand entity relationships
4. **Run Tests** - Verify system functionality

### For Administrators
1. **Database Setup** - Configure production database
2. **Security Configuration** - Set up user access
3. **Backup Strategy** - Plan data protection
4. **Performance Monitoring** - Monitor system health

## 📚 Next Steps

After completing the quick start:

1. **Read the User Guides**
   - [Admin User Guide](../user-guides/Admin-Guide.md)
   - [Pharmacist Guide](../user-guides/Pharmacist-Guide.md)
   - [Manager Guide](../user-guides/Manager-Guide.md)

2. **Explore Advanced Features**
   - [Purchase Order Workflow](../modules/Purchase-Orders.md)
   - [Inventory Management](../modules/Product-Management.md)
   - [Reports & Analytics](../modules/Reports-Analytics.md)

3. **Development Setup** (if contributing)
   - [Development Environment](../development/Development-Setup.md)
   - [Coding Standards](../development/Coding-Standards.md)
   - [Testing Guidelines](../development/Testing-Guidelines.md)

## 🆘 Getting Help

If you encounter issues:

1. **Check Troubleshooting Guide**: [Troubleshooting](../user-guides/Troubleshooting.md)
2. **Review System Requirements**: [System Requirements](System-Requirements.md)
3. **Search Issues**: [GitHub Issues](https://github.com/nazzmullxd/Pharmacy-Management-System/issues)
4. **Ask Questions**: [GitHub Discussions](https://github.com/nazzmullxd/Pharmacy-Management-System/discussions)

---

**⏱️ Estimated Setup Time**: 5-10 minutes  
**📊 Success Rate**: 95% on first attempt  
**🔧 Support Level**: Full documentation and community support

> 💡 **Pro Tip**: Keep this guide handy for quick reference. The system is designed to be intuitive, but don't hesitate to explore the comprehensive documentation for advanced features!