-- SuperShop SQL Server initialization script
-- Run this in SQL Server Management Studio (SSMS) connected to a server/instance you control.
-- Default application connection string uses LocalDB: Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SuperShopDb;Integrated Security=True;
-- To use a different server/instance, change the connection string or set the environment variable SUPER_SHOP_CONNECTION.

-- Create database if it doesn't exist
IF DB_ID(N'SuperShopDb') IS NULL
BEGIN
    CREATE DATABASE [SuperShopDb];
END
GO

USE [SuperShopDb];
GO

-- Create tables if they don't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.Users (
        UserId INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(200) NOT NULL,
        Email NVARCHAR(200) UNIQUE NOT NULL,
        Phone NVARCHAR(50),
        Address NVARCHAR(500),
        Password NVARCHAR(200) NOT NULL,
        Role NVARCHAR(50) NOT NULL,
        JoiningDate DATETIME2 DEFAULT SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.Products (
        ProductId INT IDENTITY(1,1) PRIMARY KEY,
        ProductName NVARCHAR(300) NOT NULL,
        Category NVARCHAR(100),
        Brand NVARCHAR(100),
        Model NVARCHAR(100),
        Price DECIMAL(10,2) NOT NULL,
        StockQuantity INT NOT NULL,
        MinimumStock INT NOT NULL,
        Description NVARCHAR(1000)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Cart]') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.Cart (
        CartId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT,
        ProductId INT,
        Quantity INT,
        AddedDate DATETIME2 DEFAULT SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.Bills (
        BillId INT IDENTITY(1,1) PRIMARY KEY,
        CustomerName NVARCHAR(200),
        CustomerPhone NVARCHAR(50),
        TotalAmount DECIMAL(10,2),
        DiscountAmount DECIMAL(10,2),
        FinalAmount DECIMAL(10,2),
        DeliveryStatus NVARCHAR(50) DEFAULT N'Pending',
        BillDate DATETIME2 DEFAULT SYSUTCDATETIME(),
        SalesmanId INT
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BillItems]') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.BillItems (
        BillItemId INT IDENTITY(1,1) PRIMARY KEY,
        BillId INT,
        ProductId INT,
        Quantity INT,
        UnitPrice DECIMAL(10,2),
        TotalPrice DECIMAL(10,2)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reviews]') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.Reviews (
        ReviewId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT,
        ProductId INT,
        Rating INT,
        Comment NVARCHAR(1000),
        ReviewDate DATETIME2 DEFAULT SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Offers]') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.Offers (
        OfferId INT IDENTITY(1,1) PRIMARY KEY,
        ProductId INT,
        DiscountPercent DECIMAL(5,2),
        StartDate DATETIME2,
        EndDate DATETIME2
    );
END
GO

-- Insert sample data if tables are empty
IF NOT EXISTS (SELECT 1 FROM dbo.Users)
BEGIN
    INSERT INTO dbo.Users (FullName, Email, Phone, Address, Password, Role)
    VALUES
    (N'Super Admin', N'super@admin.com', N'1234567890', N'Admin Str 1', N'admin123', N'Super Admin'),
    (N'Manager One', N'admin@store.com', N'1234567891', N'Manager Str 2', N'admin123', N'Admin'),
    (N'Sales Man', N'sales@store.com', N'1234567892', N'Sales Str 3', N'sales123', N'Salesman'),
    (N'John Customer', N'customer@store.com', N'1234567893', N'Customer Str 4', N'cust123', N'Customer');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
    INSERT INTO dbo.Products (ProductName, Category, Brand, Model, Price, StockQuantity, MinimumStock, Description)
    VALUES
    (N'Fresh Milk 1L', N'Dairy', N'FarmFresh', N'', 1.50, 120, 20, N'Pasteurized milk, 1 litre'),
    (N'Brown Bread Loaf', N'Bakery', N'BakeHouse', N'', 1.20, 80, 15, N'Whole wheat brown bread'),
    (N'Eggs 12 Pack', N'Poultry', N'HappyEggs', N'', 2.50, 200, 30, N'Free-range eggs, dozen'),
    (N'Olive Oil 1L', N'Pantry', N'Oliva', N'', 6.99, 40, 5, N'Extra virgin olive oil'),
    (N'Basmati Rice 5kg', N'Grains', N'GoldenGrain', N'', 18.00, 25, 5, N'Premium long-grain basmati rice'),
    (N'White Sugar 2kg', N'Pantry', N'SweetCo', N'', 3.50, 60, 10, N'Refined white sugar 2kg');
END
GO
