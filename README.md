# 💊 PharmacyERP Suite  
A Digital Pharmacy Management System for M/S Rabiul Pharmacy  

📦 Inventory | 💰 Sales & Purchases | 🧾 Billing | 📊 Reports | 🛡️ Compliance

---

## 🧭 Project Overview

PharmacyERP Suite is a structured pharmacy management solution developed as part of the System Analysis and Design coursework. It addresses real-world pharmacy workflows including product and batch tracking, sales/purchases, customer and supplier management, expiry monitoring, and compliance logging — tailored specifically for M/S Rabiul Pharmacy.

---

## 🚀 Features Implemented

- ✅ Product and Batch-wise Inventory Management  
- ✅ Sale and Purchase Order Modules  
- ✅ Customer and Supplier Records  
- ✅ DGDA-Compliant Antibiotic Sales Logging  
- ✅ Expiry Alerts and Expiring Product Reports  
- ✅ Daily Profit/Loss and Stock Reporting  
- ✅ Role-Based Access Control (Owner / Staff)  
- ✅ Stock Adjustments with Audit Logging  

---

## 🏗️ Tech Stack

- Language: C#  
- Framework: .NET 8 / .NET Core  
- ORM: Entity Framework Core  
- Database: SQL Server Express Edition  
- Architecture: Layered MVC (Database -> Business -> Presentation)

---

## 🗂️ Project Structure

| Folder / File           | Purpose                                 |
|-------------------------|-----------------------------------------|
| /Models                 | Entity classes (Product, Sale, etc.)    |
| /DatabaseContext        |EF Core DbContext configuration          |
| /Migrations             | Entity Framework migration files        |
| /Test                   | Test data and demo values               |
| README.md               | Project overview                        |

---

## 🛠️ Setup Instructions

1. Clone the repository:  
  [gh repo clone nazzmullxd/Pharmacy-Management-System](https://github.com/nazzmullxd/Pharmacy-Management-System.git)
2. Navigate to project directory:  
   cd PharmacyERP

3. Run EF Core migrations to set up the database:  
   dotnet ef database update

4. Launch the application:  
   dotnet run

> Recommended IDE: Visual Studio 2022 or VS Code

---

## 🔐 Entity Overview

The project includes the following core entities:

- Product, ProductBatch  
- Purchase, PurchaseItem  
- Sale, SaleItem  
- Customer, Supplier  
- AntibioticLog (DGDA compliance)  
- AuditLog (user actions)  
- User (with role-based access)

---

## 🧑‍💻 Contributors

- Lead Developer: Nazmul Huda
- Database Manager: Zahidul Islam
- UI/UX Design : Iftekhar Ahmed
- Guide : MD Omar Faruk Monem
- Academic Supervisor: Engr. ASM Shakil Ahemd 
- Pharmacy Advisor: Rabiul Pharmacy  

---

## 📄 License

This project is for academic purposes only.  
For inquiries or further use, please contact the developer.
