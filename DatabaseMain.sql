USE [Pharmacy Management System];
GO

-- 1. Users
CREATE TABLE dbo.Users (
    UserID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    LastLoginDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Role NVARCHAR(20) NOT NULL
);
GO

-- 2. Suppliers
CREATE TABLE dbo.Suppliers (
    SupplierID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    SupplierName NVARCHAR(MAX) NOT NULL,
    ContactPerson NVARCHAR(MAX) NOT NULL,
    PhoneNumber NVARCHAR(MAX) NOT NULL,
    Email NVARCHAR(MAX) NULL,
    Address NVARCHAR(MAX) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(MAX) NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
GO

-- 3. Products
CREATE TABLE dbo.Products (
    ProductID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    ProductName NVARCHAR(MAX) NOT NULL,
    GenericName NVARCHAR(MAX) NOT NULL,
    Manufacturer NVARCHAR(MAX) NOT NULL,
    Category NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    UnitPrice DECIMAL(18,2) NOT NULL CHECK ([UnitPrice] >= 0),
    DefaultRetailPrice DECIMAL(18,2) NOT NULL CHECK ([DefaultRetailPrice] >= 0),
    DefaultWholeSalePrice DECIMAL(18,2) NOT NULL CHECK ([DefaultWholeSalePrice] >= 0),
    IsActive BIT NOT NULL DEFAULT 1,
    Barcode NVARCHAR(MAX) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
     LowStockThreshold INT NOT NULL DEFAULT 0,
    TotalStock INT NOT NULL DEFAULT 0
);
GO

-- 4. Customers
CREATE TABLE dbo.Customers (
    CustomerID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CustomerName NVARCHAR(MAX) NOT NULL,
    ContactNumber NVARCHAR(MAX) NOT NULL,
    Email NVARCHAR(MAX) NULL,
    Address NVARCHAR(MAX) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- 5. ProductBatches
CREATE TABLE dbo.ProductBatches (
    ProductBatchID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    ProductID UNIQUEIDENTIFIER NOT NULL,
    SupplierID UNIQUEIDENTIFIER NOT NULL,
    BatchNumber NVARCHAR(MAX) NOT NULL,
    ExpiryDate DATETIME2 NOT NULL,
    QuantityInStock INT NOT NULL CHECK ([QuantityInStock] >= 0),
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ProductBatches_Products FOREIGN KEY (ProductID) REFERENCES dbo.Products(ProductID),
    CONSTRAINT FK_ProductBatches_Suppliers FOREIGN KEY (SupplierID) REFERENCES dbo.Suppliers(SupplierID)
);
GO

-- 6. Sales
CREATE TABLE dbo.Sales (
    SaleID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CustomerID UNIQUEIDENTIFIER NOT NULL,
    UserID UNIQUEIDENTIFIER NOT NULL,
    SaleDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    TotalAmount DECIMAL(18,2) NOT NULL CHECK ([TotalAmount] >= 0),
    PaymentSatus NVARCHAR(20) NOT NULL DEFAULT 'Paid',
    Note NVARCHAR(500) NULL,
    CONSTRAINT FK_Sales_Customers FOREIGN KEY (CustomerID) REFERENCES dbo.Customers(CustomerID),
    CONSTRAINT FK_Sales_Users FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID)
);
GO

-- 7. Purchases
CREATE TABLE dbo.Purchases (
    PurchaseID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    SupplierID UNIQUEIDENTIFIER NOT NULL,
    UserID UNIQUEIDENTIFIER NOT NULL,
    ProductBatchID UNIQUEIDENTIFIER NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL CHECK ([TotalAmount] >= 0),
    PurchaseDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Notes NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Purchases_Suppliers FOREIGN KEY (SupplierID) REFERENCES dbo.Suppliers(SupplierID),
    CONSTRAINT FK_Purchases_Users FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
    CONSTRAINT FK_Purchases_ProductBatches FOREIGN KEY (ProductBatchID) REFERENCES dbo.ProductBatches(ProductBatchID)
);
GO

-- 8. SaleItems
CREATE TABLE dbo.SaleItems (
    SalesItemID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    SaleID UNIQUEIDENTIFIER NOT NULL,
    ProductBatchID UNIQUEIDENTIFIER NOT NULL,
    ProductID UNIQUEIDENTIFIER NOT NULL,
    Quantity INT NOT NULL CHECK ([Quantity] > 0),
    UnitPrice DECIMAL(18,2) NOT NULL CHECK ([UnitPrice] > 0),
    Discount DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_SaleItems_Sales FOREIGN KEY (SaleID) REFERENCES dbo.Sales(SaleID),
    CONSTRAINT FK_SaleItems_ProductBatches FOREIGN KEY (ProductBatchID) REFERENCES dbo.ProductBatches(ProductBatchID),
    CONSTRAINT FK_SaleItems_Products FOREIGN KEY (ProductID) REFERENCES dbo.Products(ProductID)
);
GO

-- 9. PurchaseItems
CREATE TABLE dbo.PurchaseItems (
    PurchaseItemID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    PurchaseID UNIQUEIDENTIFIER NOT NULL,
    ProductBatchID UNIQUEIDENTIFIER NOT NULL,
    ProductID UNIQUEIDENTIFIER NOT NULL,
    Quantity INT NOT NULL CHECK ([Quantity] >= 0),
    UnitPrice DECIMAL(18,2) NOT NULL CHECK ([UnitPrice] >= 0),
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PurchaseItems_Purchases FOREIGN KEY (PurchaseID) REFERENCES dbo.Purchases(PurchaseID),
    CONSTRAINT FK_PurchaseItems_ProductBatches FOREIGN KEY (ProductBatchID) REFERENCES dbo.ProductBatches(ProductBatchID),
    CONSTRAINT FK_PurchaseItems_Products FOREIGN KEY (ProductID) REFERENCES dbo.Products(ProductID)
);
GO

-- 10. AuditLogs
CREATE TABLE dbo.AuditLogs (
    AuditLogID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Action NVARCHAR(MAX) NOT NULL,
    Details NVARCHAR(MAX) NULL,
    ActionDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserID)
);
GO

-- 11. AntibioticLogs
CREATE TABLE dbo.AntibioticLogs (
    AntibioticLogID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    SaleID UNIQUEIDENTIFIER NOT NULL,
    ProductBatchID UNIQUEIDENTIFIER NOT NULL,
    ProductID UNIQUEIDENTIFIER NOT NULL,
    CustomerID UNIQUEIDENTIFIER NOT NULL,
    DoctorName NVARCHAR(MAX) NOT NULL,
    PrescriptionDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_AntibioticLogs_Sales FOREIGN KEY (SaleID) REFERENCES dbo.Sales(SaleID),
    CONSTRAINT FK_AntibioticLogs_ProductBatches FOREIGN KEY (ProductBatchID) REFERENCES dbo.ProductBatches(ProductBatchID),
    CONSTRAINT FK_AntibioticLogs_Products FOREIGN KEY (ProductID) REFERENCES dbo.Products(ProductID),
    CONSTRAINT FK_AntibioticLogs_Customers FOREIGN KEY (CustomerID) REFERENCES dbo.Customers(CustomerID)
);
GO


USE [PharmacyManagementSystem];
GO

-- Step 1: Drop foreign key constraints that reference dbo.Users
ALTER TABLE dbo.Sales DROP CONSTRAINT FK_Sales_Users;
ALTER TABLE dbo.Purchases DROP CONSTRAINT FK_Purchases_Users;
ALTER TABLE dbo.AuditLogs DROP CONSTRAINT FK_AuditLogs_Users;
GO

-- Step 2: Rename the table from Users to UserInfo
EXEC sp_rename 'dbo.Users', 'UserInfo';
GO

-- Step 3: Recreate the foreign keys pointing to the renamed table
ALTER TABLE dbo.Sales
ADD CONSTRAINT FK_Sales_UserInfo FOREIGN KEY (UserID) REFERENCES dbo.UserInfo(UserID);
GO

ALTER TABLE dbo.Purchases
ADD CONSTRAINT FK_Purchases_UserInfo FOREIGN KEY (UserID) REFERENCES dbo.UserInfo(UserID);
GO

ALTER TABLE dbo.AuditLogs
ADD CONSTRAINT FK_AuditLogs_UserInfo FOREIGN KEY (UserId) REFERENCES dbo.UserInfo(UserID);
GO



