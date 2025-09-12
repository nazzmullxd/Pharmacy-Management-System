USE [Pharmacy Management System];
GO

-- 1. UsersInfo
CREATE TABLE [dbo].[UsersInfo] (
    [UserID]        UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [FirstName]     NVARCHAR (50)    NOT NULL,
    [LastName]      NVARCHAR (50)    NOT NULL,
    [Email]         NVARCHAR (255)   NOT NULL,
    [PasswordHash]  NVARCHAR (255)   NOT NULL,
    [LastLoginDate] DATETIME2 (7)    DEFAULT (sysutcdatetime()) NOT NULL,
    [Role]          NVARCHAR (20)    NOT NULL,
    [CreatedDate]   DATETIME2 (7)    DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedDate]   DATETIME2 (7)    DEFAULT (sysutcdatetime()) NOT NULL,
    PRIMARY KEY CLUSTERED ([UserID] ASC)
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
CREATE TABLE [dbo].[Sales] (
    [SaleID]       UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [CustomerID]   UNIQUEIDENTIFIER NOT NULL,
    [UserID]       UNIQUEIDENTIFIER NOT NULL,
    [SaleDate]     DATETIME2 (7)    DEFAULT (sysutcdatetime()) NOT NULL,
    [TotalAmount]  DECIMAL (18, 2)  NOT NULL,
    [PaymentStatus] NVARCHAR (20)    DEFAULT ('Paid') NULL,
    [Note]         NVARCHAR (500)   NULL,
    PRIMARY KEY CLUSTERED ([SaleID] ASC),
    CONSTRAINT [FK_Sales_UsersInfo] FOREIGN KEY ([UserID]) REFERENCES [dbo].[UsersInfo] ([UserID]),
    CONSTRAINT [FK_Sales_Customers] FOREIGN KEY ([CustomerID]) REFERENCES [dbo].[Customers] ([CustomerID]),
    CHECK ([TotalAmount]>=(0))
);
GO

-- 7. Purchases
CREATE TABLE [dbo].[Purchases] (
    [PurchaseID]     UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [SupplierID]     UNIQUEIDENTIFIER NOT NULL,
    [UserID]         UNIQUEIDENTIFIER NOT NULL,
    [ProductBatchID] UNIQUEIDENTIFIER NOT NULL,
    [TotalAmount]    DECIMAL (18, 2)  NOT NULL,
    [PurchaseDate]   DATETIME2 (7)    DEFAULT (sysutcdatetime()) NOT NULL,
    [CreatedDate]    DATETIME2 (7)    DEFAULT (sysutcdatetime()) NOT NULL,
    [Notes]          NVARCHAR (MAX)   NULL,
    PRIMARY KEY CLUSTERED ([PurchaseID] ASC),
    CONSTRAINT [FK_Purchases_UsersInfo] FOREIGN KEY ([UserID]) REFERENCES [dbo].[UsersInfo] ([UserID]),
    CONSTRAINT [FK_Purchases_Suppliers] FOREIGN KEY ([SupplierID]) REFERENCES [dbo].[Suppliers] ([SupplierID]),
    CONSTRAINT [FK_Purchases_ProductBatches] FOREIGN KEY ([ProductBatchID]) REFERENCES [dbo].[ProductBatches] ([ProductBatchID]),
    CHECK ([TotalAmount]>=(0))
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
CREATE TABLE [dbo].[AuditLogs] (
    [AuditLogID] UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [UserId]     UNIQUEIDENTIFIER NOT NULL,
    [Action]     NVARCHAR (MAX)   NOT NULL,
    [Details]    NVARCHAR (MAX)   NULL,
    [ActionDate] DATETIME2 (7)    DEFAULT (sysutcdatetime()) NOT NULL,
    PRIMARY KEY CLUSTERED ([AuditLogID] ASC),
    CONSTRAINT [FK_AuditLogs_UsersInfo] FOREIGN KEY ([UserId]) REFERENCES [dbo].[UsersInfo] ([UserID])
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


