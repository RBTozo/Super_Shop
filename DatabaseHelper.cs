using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;

namespace SuperShop
{
    public static class DatabaseHelper
    {
        // Use environment variable SUPER_SHOP_CONNECTION to override connection string.
        // Default uses LocalDB so the user can manage the DB with SQL Server Management Studio.
        private static string GetConnectionString()
        {
            var env = Environment.GetEnvironmentVariable("SUPER_SHOP_CONNECTION");
            if (!string.IsNullOrWhiteSpace(env))
            {
                // Fix corrupted batch file string if present
                if (env.Contains("^(localdb^)"))
                {
                    env = env.Replace("^(localdb^)", "(localdb)");
                }
                return env;
            }

            // Check for dbconfig.json in common runtime locations.
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string currentDir = Directory.GetCurrentDirectory();
                string[] candidates =
                {
                    Path.Combine(baseDir, "dbconfig.json"),
                    Path.Combine(currentDir, "dbconfig.json"),
                    Path.Combine(Directory.GetParent(baseDir)?.FullName ?? baseDir, "dbconfig.json")
                };

                foreach (var cfgPath in candidates)
                {
                    if (!File.Exists(cfgPath)) continue;

                    var json = File.ReadAllText(cfgPath);
                    using var doc = System.Text.Json.JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("ConnectionString", out var cs))
                    {
                        var s = cs.GetString();
                        if (!string.IsNullOrWhiteSpace(s)) return s;
                    }
                }
            }
            catch { }

            return "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SuperShopDb;Integrated Security=True;TrustServerCertificate=True;";
        }

        public static bool VerifyConnection(out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using var conn = GetConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 5;
                var _ = cmd.ExecuteScalar();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        private static string GetMasterConnectionString()
        {
            var builder = new SqlConnectionStringBuilder(GetConnectionString());
            builder.InitialCatalog = "master";
            return builder.ConnectionString;
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(GetConnectionString());
        }

        public static void InitializeDatabase()
        {
            var builder = new SqlConnectionStringBuilder(GetConnectionString());
            string dbName = builder.InitialCatalog;

            // Ensure database exists (connect to master and create if missing)
            using (var masterConn = new SqlConnection(GetMasterConnectionString()))
            {
                masterConn.Open();
                using var cmd = masterConn.CreateCommand();
                cmd.CommandText = $"IF DB_ID(N'{dbName}') IS NULL CREATE DATABASE [{dbName}];";
                cmd.ExecuteNonQuery();
            }

            // Create tables if they don't exist and insert sample data if empty
            using (var conn = GetConnection())
            {
                conn.Open();
                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
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
";
                cmd.ExecuteNonQuery();

                // Insert sample data only if tables are empty
                cmd.CommandText = "SELECT COUNT(1) FROM dbo.Users";
                var userCount = (int)cmd.ExecuteScalar();
                if (userCount == 0)
                {
                    InsertSampleData(conn);
                }
            }
        }

        // Helper methods for reviews, offers and delivery status
        public static double GetAverageRating(int productId)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT AVG(CAST(Rating AS FLOAT)) FROM Reviews WHERE ProductId = @pid";
                cmd.Parameters.AddWithValue("@pid", productId);
                var res = cmd.ExecuteScalar();
                if (res == DBNull.Value || res == null) return 0.0;
                return Convert.ToDouble(res);
            }
            catch { return 0.0; }
        }

        public static decimal GetActiveOfferPercent(int productId)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT TOP 1 DiscountPercent FROM Offers WHERE ProductId = @pid AND (@now BETWEEN StartDate AND EndDate) ORDER BY DiscountPercent DESC";
                cmd.Parameters.AddWithValue("@pid", productId);
                cmd.Parameters.AddWithValue("@now", DateTime.Now);
                var res = cmd.ExecuteScalar();
                if (res == DBNull.Value || res == null) return 0m;
                return Convert.ToDecimal(res);
            }
            catch { return 0m; }
        }

        public static void InsertReview(int userId, int productId, int rating, string comment)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Reviews (UserId, ProductId, Rating, Comment) VALUES (@uid,@pid,@r,@c)";
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@pid", productId);
            cmd.Parameters.AddWithValue("@r", rating);
            cmd.Parameters.AddWithValue("@c", comment ?? string.Empty);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateBillDeliveryStatus(int billId, string status)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Bills SET DeliveryStatus = @s WHERE BillId = @id";
            cmd.Parameters.AddWithValue("@s", status ?? "");
            cmd.Parameters.AddWithValue("@id", billId);
            cmd.ExecuteNonQuery();
        }

        private static void InsertSampleData(SqlConnection conn)
        {
            using var tran = conn.BeginTransaction();
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;

                // Sample Users
                cmd.CommandText = "INSERT INTO dbo.Users (FullName, Email, Phone, Address, Password, Role) VALUES (@n,@e,@p,@a,@pw,@r);";
                cmd.Parameters.Add(new SqlParameter("@n", SqlDbType.NVarChar));
                cmd.Parameters.Add(new SqlParameter("@e", SqlDbType.NVarChar));
                cmd.Parameters.Add(new SqlParameter("@p", SqlDbType.NVarChar));
                cmd.Parameters.Add(new SqlParameter("@a", SqlDbType.NVarChar));
                cmd.Parameters.Add(new SqlParameter("@pw", SqlDbType.NVarChar));
                cmd.Parameters.Add(new SqlParameter("@r", SqlDbType.NVarChar));

                void ExecUser(string name, string email, string phone, string addr, string pw, string role)
                {
                    cmd.Parameters[0].Value = name;
                    cmd.Parameters[1].Value = email;
                    cmd.Parameters[2].Value = phone;
                    cmd.Parameters[3].Value = addr;
                    cmd.Parameters[4].Value = pw;
                    cmd.Parameters[5].Value = role;
                    cmd.ExecuteNonQuery();
                }

                ExecUser("Super Admin", "super@admin.com", "1234567890", "Admin Str 1", "admin123", "Super Admin");
                ExecUser("Manager One", "admin@store.com", "1234567891", "Manager Str 2", "admin123", "Admin");
                ExecUser("Sales Man", "sales@store.com", "1234567892", "Sales Str 3", "sales123", "Salesman");
                ExecUser("John Customer", "customer@store.com", "1234567893", "Customer Str 4", "cust123", "Customer");

                // Products
                cmd.Parameters.Clear();
                cmd.CommandText = "INSERT INTO dbo.Products (ProductName, Category, Brand, Model, Price, StockQuantity, MinimumStock, Description) VALUES (@pn,@cat,@brand,@model,@price,@stock,@min,@desc);";
                cmd.Parameters.Add(new SqlParameter("@pn", SqlDbType.NVarChar));
                cmd.Parameters.Add(new SqlParameter("@cat", SqlDbType.NVarChar));
                cmd.Parameters.Add(new SqlParameter("@brand", SqlDbType.NVarChar));
                cmd.Parameters.Add(new SqlParameter("@model", SqlDbType.NVarChar));
                cmd.Parameters.Add(new SqlParameter("@price", SqlDbType.Decimal));
                cmd.Parameters.Add(new SqlParameter("@stock", SqlDbType.Int));
                cmd.Parameters.Add(new SqlParameter("@min", SqlDbType.Int));
                cmd.Parameters.Add(new SqlParameter("@desc", SqlDbType.NVarChar));

                void ExecProduct(string name, string cat, string brand, string model, decimal price, int stock, int min, string desc)
                {
                    cmd.Parameters[0].Value = name;
                    cmd.Parameters[1].Value = cat;
                    cmd.Parameters[2].Value = brand;
                    cmd.Parameters[3].Value = model;
                    cmd.Parameters[4].Value = price;
                    cmd.Parameters[5].Value = stock;
                    cmd.Parameters[6].Value = min;
                    cmd.Parameters[7].Value = desc;
                    cmd.ExecuteNonQuery();
                }

                ExecProduct("Fresh Milk 1L", "Dairy", "FarmFresh", "", 1.50m, 120, 20, "Pasteurized milk, 1 litre");
                ExecProduct("Brown Bread Loaf", "Bakery", "BakeHouse", "", 1.20m, 80, 15, "Whole wheat brown bread");
                ExecProduct("Eggs 12 Pack", "Poultry", "HappyEggs", "", 2.50m, 200, 30, "Free-range eggs, dozen");
                ExecProduct("Olive Oil 1L", "Pantry", "Oliva", "", 6.99m, 40, 5, "Extra virgin olive oil");
                ExecProduct("Basmati Rice 5kg", "Grains", "GoldenGrain", "", 18.00m, 25, 5, "Premium long-grain basmati rice");
                ExecProduct("White Sugar 2kg", "Pantry", "SweetCo", "", 3.50m, 60, 10, "Refined white sugar 2kg");

                tran.Commit();
            }
        }
    }
}
