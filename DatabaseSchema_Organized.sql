-- =====================================================================================
-- Pharmacy Management System - Database Schema
-- Organized by dependency order: Parent tables first, then child tables
-- =====================================================================================

-- =====================================================================================
-- 1. CORE SYSTEM TABLES (No dependencies)
-- =====================================================================================

-- Entity Framework Migrations History
CREATE TABLE [dbo].[__EFMigrationsHistory] (
    [MigrationId]    NVARCHAR(150) NOT NULL,
    [ProductVersion] NVARCHAR(32)  NOT NULL,
    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED ([MigrationId] ASC)
);

-- User Management
CREATE TABLE [dbo].[UsersInfo] (
    [UserID]        UNIQUEIDENTIFIER NOT NULL DEFAULT (newid()),
    [FirstName]     NVARCHAR(50)     NOT NULL,
    [LastName]      NVARCHAR(50)     NOT NULL,
    [Email]         NVARCHAR(255)    NOT NULL,
    [PasswordHash]  NVARCHAR(255)    NOT NULL,
    [Role]          NVARCHAR(20)     NOT NULL,
    [LastLoginDate] DATETIME2(7)     NOT NULL DEFAULT (sysutcdatetime()),
    [CreatedDate]   DATETIME2(7)     NOT NULL DEFAULT (sysutcdatetime()),
    [UpdatedDate]   DATETIME2(7)     NOT NULL DEFAULT (sysutcdatetime()),
    CONSTRAINT [PK_UsersInfo] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

-- Customer Management
CREATE TABLE [dbo].[Customers] (
    [CustomerID]    UNIQUEIDENTIFIER NOT NULL DEFAULT (newid()),
    [CustomerName]  NVARCHAR(MAX)    NOT NULL,
    [ContactNumber] NVARCHAR(MAX)    NOT NULL,
    [Email]         NVARCHAR(MAX)    NULL,
    [Address]       NVARCHAR(MAX)    NULL,
    [CreatedDate]   DATETIME2(7)     NOT NULL DEFAULT (sysutcdatetime()),
    CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED ([CustomerID] ASC)
);

-- Supplier Management
CREATE TABLE [dbo].[Suppliers] (
    [SupplierID]    UNIQUEIDENTIFIER NOT NULL DEFAULT (newid()),
    [SupplierName]  NVARCHAR(MAX)    NOT NULL,
    [ContactPerson] NVARCHAR(MAX)    NOT NULL,
    [PhoneNumber]   NVARCHAR(MAX)    NOT NULL,
    [Email]         NVARCHAR(MAX)    NULL,
    [Address]       NVARCHAR(MAX)    NULL,
    [CreatedBy]     NVARCHAR(MAX)    NULL,
    [IsActive]      BIT              NOT NULL DEFAULT (1),
    [CreatedDate]   DATETIME2(7)     NOT NULL DEFAULT (sysutcdatetime()),
    [UpdatedDate]   DATETIME2(7)     NOT NULL DEFAULT (sysutcdatetime()),
    CONSTRAINT [PK_Suppliers] PRIMARY KEY CLUSTERED ([SupplierID] ASC)
);

-- =====================================================================================
-- 2. PRODUCT MANAGEMENT TABLES
-- =====================================================================================

-- Product Catalog
CREATE TABLE [dbo].[Products] (
    [ProductID]             UNIQUEIDENTIFIER NOT NULL DEFAULT (newid()),
    [ProductName]           NVARCHAR(MAX)    NOT NULL,
    [GenericName]           NVARCHAR(MAX)    NOT NULL,
    [Manufacturer]          NVARCHAR(MAX)    NOT NULL,
    [Category]              NVARCHAR(MAX)    NOT NULL,
    [Description]           NVARCHAR(MAX)    NULL,
    [UnitPrice]             DECIMAL(18, 2)   NOT NULL,
    [DefaultRetailPrice]    DECIMAL(18, 2)   NOT NULL,
    [DefaultWholeSalePrice] DECIMAL(18, 2)   NOT NULL,
    [Barcode]               NVARCHAR(MAX)    NULL,
    [IsActive]              BIT              NOT NULL DEFAULT (1),
    [LowStockThreshold]     INT              NOT NULL DEFAULT (0),
    [TotalStock]            INT              NOT NULL DEFAULT (0),
    [CreatedDate]           DATETIME2(7)     NOT NULL DEFAULT (sysutcdatetime()),
    CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([ProductID] ASC),
    CONSTRAINT [CK_Products_UnitPrice] CHECK ([UnitPrice] >= 0),
    CONSTRAINT [CK_Products_RetailPrice] CHECK ([DefaultRetailPrice] >= 0),
    CONSTRAINT [CK_Products_WholesalePrice] CHECK ([DefaultWholeSalePrice] >= 0)
);

-- Product Batch Management (Depends on Products, Suppliers)
CREATE TABLE [dbo].[ProductBatches] (
    [ProductBatchID]  UNIQUEIDENTIFIER NOT NULL DEFAULT (newid()),
    [ProductID]       UNIQUEIDENTIFIER NOT NULL,
    [SupplierID]      UNIQUEIDENTIFIER NOT NULL,
    [BatchNumber]     NVARCHAR(MAX)    NOT NULL,
    [ExpiryDate]      DATETIME2(7)     NOT NULL,
    [QuantityInStock] INT              NOT NULL,
    [CreatedDate]     DATETIME2(7)     NOT NULL DEFAULT (sysutcdatetime()),
    CONSTRAINT [PK_ProductBatches] PRIMARY KEY CLUSTERED ([ProductBatchID] ASC),
    CONSTRAINT [CK_ProductBatches_Quantity] CHECK ([QuantityInStock] >= 0),
    CONSTRAINT [FK_ProductBatches_Products] FOREIGN KEY ([ProductID]) 
        REFERENCES [dbo].[Products] ([ProductID]),
    CONSTRAINT [FK_ProductBatches_Suppliers] FOREIGN KEY ([SupplierID]) 
        REFERENCES [dbo].[Suppliers] ([SupplierID])
);

-- =====================================================================================
-- 3. PURCHASE MANAGEMENT TABLES
-- =====================================================================================

-- Purchase Orders (Depends on Suppliers, Users, ProductBatches)
CREATE TABLE [dbo].[Purchases] (
    [PurchaseID]     UNIQUEIDENTIFIER NOT NULL DEFAULT (newid()),
    [SupplierID]     UNIQUEIDENTIFIER NOT NULL,
    [UserID]         UNIQUEIDENTIFIER NULL,
    [ProductBatchID] UNIQUEIDENTIFIER NOT NULL,
    [OrderNumber]    NVARCHAR(50)     NULL,
    [TotalAmount]    DECIMAL(18, 2)   NOT NULL,
    [PaidAmount]     DECIMAL(18, 2)   NOT NULL DEFAULT (0),
    [DueAmount]      DECIMAL(18, 2)   NOT NULL DEFAULT (0),
    [Status]         NVARCHAR(50)     NOT NULL DEFAULT ('Pending'),
    [PaymentStatus]  NVARCHAR(50)     NOT NULL DEFAULT ('Pending'),
    [Notes]          NVARCHAR(MAX)    NULL,
    [OrderDate]      DATETIME2(7)     NOT NULL DEFAULT (sysutcdatetime()),
    [PurchaseDate]   DATETIME2(7)     NOT NULL DEFAULT (sysutcdatetime()),
    [CreatedDate]    DATETIME2(7)     NOT NULL DEFAULT (sysutcdatetime()),
    CONSTRAINT [PK_Purchases] PRIMARY KEY CLUSTERED ([PurchaseID] ASC),
    CONSTRAINT [CK_Purchases_TotalAmount] CHECK ([TotalAmount] >= 0),
    CONSTRAINT [FK_Purchases_Suppliers] FOREIGN KEY ([SupplierID]) 
        REFERENCES [dbo].[Suppliers] ([SupplierID])
);

-- Purchase Order Items (Depends on Purchases, Products, ProductBatches)
CREATE TABLE [dbo].[PurchaseItems] (
    [PurchaseItemID] UNIQUEIDENTIFIER NOT NULL DEFAULT (newid()),
    [PurchaseID]     UNIQUEIDENTIFIER NOT NULL,
    [ProductBatchID] UNIQUEIDENTIFIER NOT NULL,
    [ProductID]      UNIQUEIDENTIFIER NOT NULL,
    [Quantity]       INT              NOT NULL,
    [UnitPrice]      DECIMAL(18, 2)   NOT NULL,
    [CreatedDate]    DATETIME2(7)     NOT NULL DEFAULT (sysutcdatetime()),
    CONSTRAINT [PK_PurchaseItems] PRIMARY KEY CLUSTERED ([PurchaseItemID] ASC),
    CONSTRAINT [CK_PurchaseItems_Quantity] CHECK ([Quantity] >= 0),
    CONSTRAINT [CK_PurchaseItems_UnitPrice] CHECK ([UnitPrice] >= 0),
    CONSTRAINT [FK_PurchaseItems_Purchases] FOREIGN KEY ([PurchaseID]) 
        REFERENCES [dbo].[Purchases] ([PurchaseID]),
    CONSTRAINT [FK_PurchaseItems_ProductBatches] FOREIGN KEY ([ProductBatchID]) 
        REFERENCES [dbo].[ProductBatches] ([ProductBatchID]),
    CONSTRAINT [FK_PurchaseItems_Products] FOREIGN KEY ([ProductID]) 
        REFERENCES [dbo].[Products] ([ProductID])
);

-- =====================================================================================
-- 4. SALES MANAGEMENT TABLES
-- =====================================================================================

-- Sales Transactions (Depends on Customers, Users)
CREATE TABLE [dbo].[Sales] (
    [SaleID]        UNIQUEIDENTIFIER NOT NULL DEFAULT (newid()),
    [CustomerID]    UNIQUEIDENTIFIER NULL,  -- Made nullable for walk-in customers
    [UserID]        UNIQUEIDENTIFIER NOT NULL,
    [TotalAmount]   DECIMAL(18, 2)   NOT NULL,
    [PaymentStatus] NVARCHAR(20)     NULL DEFAULT ('Paid'),  -- Made nullable
    [Note]          NVARCHAR(500)    NULL,
    [SaleDate]      DATETIME2(7)     NOT NULL DEFAULT (sysutcdatetime()),
    CONSTRAINT [PK_Sales] PRIMARY KEY CLUSTERED ([SaleID] ASC),
    CONSTRAINT [CK_Sales_TotalAmount] CHECK ([TotalAmount] >= 0),
    CONSTRAINT [FK_Sales_UsersInfo] FOREIGN KEY ([UserID]) 
        REFERENCES [dbo].[UsersInfo] ([UserID]),
    CONSTRAINT [FK_Sales_Customers] FOREIGN KEY ([CustomerID]) 
        REFERENCES [dbo].[Customers] ([CustomerID])
);

-- Sales Items (Depends on Sales, Products, ProductBatches)
CREATE TABLE [dbo].[SalesItems] (
    [SaleItemID]     UNIQUEIDENTIFIER NOT NULL DEFAULT (newid()),
    [SaleID]         UNIQUEIDENTIFIER NOT NULL,
    [ProductBatchID] UNIQUEIDENTIFIER NULL,
    [ProductID]      UNIQUEIDENTIFIER NOT NULL,
    [Quantity]       INT              NOT NULL,
    [UnitPrice]      DECIMAL(18, 2)   NOT NULL,
    [Discount]       DECIMAL(18, 2)   NOT NULL DEFAULT (0),
    [BatchNumber]    VARCHAR(50)      NULL,
    [CreatedDate]    DATETIME2(7)     NOT NULL DEFAULT (sysutcdatetime()),
    CONSTRAINT [PK_SalesItems] PRIMARY KEY CLUSTERED ([SaleItemID] ASC),
    CONSTRAINT [CK_SalesItems_Quantity] CHECK ([Quantity] > 0),
    CONSTRAINT [CK_SalesItems_UnitPrice] CHECK ([UnitPrice] > 0),
    CONSTRAINT [FK_SaleItems_Sales] FOREIGN KEY ([SaleID]) 
        REFERENCES [dbo].[Sales] ([SaleID]),
    CONSTRAINT [FK_SaleItems_Products] FOREIGN KEY ([ProductID]) 
        REFERENCES [dbo].[Products] ([ProductID])
);

-- =====================================================================================
-- 5. INVENTORY MANAGEMENT TABLES
-- =====================================================================================

-- Stock Adjustments (Depends on ProductBatches, Users)
CREATE TABLE [dbo].[StockAdjustments] (
    [StockAdjustmentID]  UNIQUEIDENTIFIER NOT NULL DEFAULT (newid()),
    [ProductBatchID]     UNIQUEIDENTIFIER NOT NULL,
    [UserID]             UNIQUEIDENTIFIER NOT NULL,
    [ApprovedBy]         UNIQUEIDENTIFIER NULL,
    [PreviousQuantity]   INT              NOT NULL,
    [AdjustedQuantity]   INT              NOT NULL,
    [QuantityDifference] INT              NOT NULL,
    [AdjustmentType]     NVARCHAR(255)    NOT NULL,
    [Reason]             NVARCHAR(MAX)    NULL,
    [IsApproved]         BIT              NOT NULL DEFAULT (0),
    [AdjustmentDate]     DATETIME2(7)     NOT NULL DEFAULT (sysutcdatetime()),
    [ApprovalDate]       DATETIME2(7)     NULL,
    CONSTRAINT [PK_StockAdjustments] PRIMARY KEY CLUSTERED ([StockAdjustmentID] ASC),
    CONSTRAINT [FK_StockAdjustments_ProductBatch] FOREIGN KEY ([ProductBatchID]) 
        REFERENCES [dbo].[ProductBatches] ([ProductBatchID])
);

-- =====================================================================================
-- 6. COMPLIANCE AND AUDIT TABLES
-- =====================================================================================

-- Antibiotic Sales Tracking (Depends on Sales, Products, ProductBatches, Customers)
CREATE TABLE [dbo].[AntibioticLogs] (
    [AntibioticLogID]  UNIQUEIDENTIFIER NOT NULL DEFAULT (newid()),
    [SaleID]           UNIQUEIDENTIFIER NOT NULL,
    [ProductBatchID]   UNIQUEIDENTIFIER NOT NULL,
    [ProductID]        UNIQUEIDENTIFIER NOT NULL,
    [CustomerID]       UNIQUEIDENTIFIER NOT NULL,
    [DoctorName]       NVARCHAR(MAX)    NOT NULL,
    [PrescriptionDate] DATETIME2(7)     NOT NULL DEFAULT (sysutcdatetime()),
    CONSTRAINT [PK_AntibioticLogs] PRIMARY KEY CLUSTERED ([AntibioticLogID] ASC),
    CONSTRAINT [FK_AntibioticLogs_Sales] FOREIGN KEY ([SaleID]) 
        REFERENCES [dbo].[Sales] ([SaleID]),
    CONSTRAINT [FK_AntibioticLogs_ProductBatches] FOREIGN KEY ([ProductBatchID]) 
        REFERENCES [dbo].[ProductBatches] ([ProductBatchID]),
    CONSTRAINT [FK_AntibioticLogs_Products] FOREIGN KEY ([ProductID]) 
        REFERENCES [dbo].[Products] ([ProductID]),
    CONSTRAINT [FK_AntibioticLogs_Customers] FOREIGN KEY ([CustomerID]) 
        REFERENCES [dbo].[Customers] ([CustomerID])
);

-- System Audit Trail (Depends on Users)
CREATE TABLE [dbo].[AuditLogs] (
    [AuditLogID] UNIQUEIDENTIFIER NOT NULL DEFAULT (newid()),
    [UserId]     UNIQUEIDENTIFIER NOT NULL,
    [Action]     NVARCHAR(MAX)    NOT NULL,
    [Details]    NVARCHAR(MAX)    NULL,
    [ActionDate] DATETIME2(7)     NOT NULL DEFAULT (sysutcdatetime()),
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY CLUSTERED ([AuditLogID] ASC),
    CONSTRAINT [FK_AuditLogs_UsersInfo] FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[UsersInfo] ([UserID])
);

-- =====================================================================================
-- END OF SCHEMA
-- =====================================================================================