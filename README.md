# Pharmacy Management System

A comprehensive, modern pharmacy management system built with ASP.NET Core Razor Pages, featuring a complete business logic layer and responsive UI/UX.

## 🏥 Features

### Core Business Modules
- **Products Management**: Complete product catalog with inventory tracking
- **Sales Management**: Sales transactions, invoicing, and payment tracking
- **Stock Management**: Inventory monitoring, low stock alerts, and expiry tracking
- **Customer Management**: Customer database and relationship management
- **Dashboard**: Real-time business metrics and KPIs

### Technical Features
- **Modern UI/UX**: Responsive design with interactive elements
- **Business Logic Layer**: Complete service layer with DTOs and interfaces
- **Database Integration**: Entity Framework with repository pattern
- **Real-time Data**: Dynamic dashboard with live business metrics
- **Search & Filtering**: Advanced search and filtering capabilities

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or Express)
- Visual Studio Code or Visual Studio

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/YOUR_USERNAME/Pharmacy-Management-System.git
   cd Pharmacy-Management-System
   ```

2. **Restore packages**:
   ```bash
   dotnet restore
   ```

3. **Update database**:
   ```bash
   cd Web
   dotnet ef database update
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

5. **Open in browser**:
   - Navigate to http://localhost:5067

## 🏗️ Project Structure

```
Pharmacy-Management-System/
├── Business/                 # Business Logic Layer
│   ├── DTOs/                # Data Transfer Objects
│   ├── Interfaces/          # Service Interfaces
│   ├── Services/            # Service Implementations
│   └── ServiceRegistration.cs
├── Database/                # Data Access Layer
│   ├── Context/             # Entity Framework Context
│   ├── Interfaces/          # Repository Interfaces
│   ├── Model/               # Entity Models
│   └── Repositories/        # Repository Implementations
└── Web/                     # Presentation Layer
    ├── Pages/               # Razor Pages
    ├── wwwroot/             # Static Files
    └── Program.cs           # Application Entry Point
```

## 🎯 Key Features

### Dashboard
- Real-time business metrics
- Sales and revenue tracking
- Stock value monitoring
- Expiring products alerts
- Top selling products

### Products Management
- Product catalog with categories
- Inventory tracking
- Low stock alerts
- Barcode management
- Price management (unit, retail, wholesale)

### Sales Management
- Sales transaction processing
- Payment status tracking
- Customer invoicing
- Sales reporting
- Transaction history

### Stock Management
- Real-time inventory levels
- Batch tracking with expiry dates
- Stock adjustments
- Low stock alerts
- Expiry monitoring

### Customer Management
- Customer database
- Contact information management
- Sales history tracking
- Customer relationship management

## 🛠️ Technologies Used

- **Backend**: ASP.NET Core 8.0, C#
- **Frontend**: Razor Pages, HTML5, CSS3, JavaScript
- **Database**: SQL Server, Entity Framework Core
- **UI Framework**: Bootstrap 5, Font Awesome
- **Charts**: Chart.js
- **Architecture**: Clean Architecture, Repository Pattern

## 📊 Business Logic

The system implements a complete business logic layer with:
- **DTOs**: Data Transfer Objects for clean data flow
- **Services**: Business service implementations
- **Interfaces**: Service contracts for dependency injection
- **Validation**: Business rule validation
- **Error Handling**: Comprehensive exception handling

## 🎨 UI/UX Features

- **Responsive Design**: Works on desktop, tablet, and mobile
- **Modern Interface**: Clean, professional design
- **Interactive Elements**: Search, filtering, sorting, pagination
- **Real-time Updates**: Live data refresh and notifications
- **Accessibility**: User-friendly interface with proper navigation

## 📈 Future Enhancements

- [ ] API Controllers for mobile app integration
- [ ] Authentication and authorization system
- [ ] Advanced reporting and analytics
- [ ] Barcode scanning integration
- [ ] Email notifications
- [ ] Multi-location support

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**Nasim Ahmed**
- GitHub: [@yourusername](https://github.com/yourusername)

## 🙏 Acknowledgments

- Built with ASP.NET Core and modern web technologies
- Inspired by real-world pharmacy management needs
- Designed for scalability and maintainability