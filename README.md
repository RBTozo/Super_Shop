# Super Shop — Grocery Shop Management System

A small Windows Forms point-of-sale and shop management application for small grocery/convenience stores. This repository contains the app, database initialization scripts, and an auto-setup script to publish and initialize the database on a new machine.

## Quick overview

- Platform: .NET 8 (Windows Forms)
- Data store: SQL Server / LocalDB (default)
- UI: Windows Forms (Customer, Salesman, Admin, Super Admin)
- Installer helper: `AUTO_SETUP.bat` automates DB creation, publish, and config

## App functionality

- Browse products, add to cart, place orders (Customer & Salesman flows).
- Create, update and remove products (Admin).
- User management (Super Admin).
- Billing and bill itemization; stock updates on checkout.
- Customer reviews and ratings per product.
- Time-limited offers (discounts) applied at checkout.
- Delivery status tracking for bills (Pending → Processing → Shipped → Delivered).

## Features added

- **Review & rating**: customers can submit a rating (1–5) and comment; average rating shown on product lists. All users can view past reviews via the "View Reviews" button (`ViewReviewsForm.cs`).
- **Discount offers**: `Offers` table supports product-level discounts with `StartDate`/`EndDate`; active offers are applied automatically during checkout and shown in the Cart.
- **Delivery status**: each bill stores a `DeliveryStatus` (default `Pending`). Admins can update it from `ManageBillsForm`. Customers and Salesmen can also track their orders and mark them as "Received" or "Delivered" respectively via the "My Orders" window (`ViewOrdersForm.cs`).
- **Auto-setup & Publish**: `AUTO_SETUP.bat` builds/publishes the app as a **self-contained single-file executable**, initializes the database using `sql/SuperShop_Init.sql`, writes `dbconfig.json` correctly without escape character bugs, and sets `SUPER_SHOP_CONNECTION`.

## Database schema (summary)

Tables and important columns (see `sql/SuperShop_Init.sql` for full DDL):

- `Users`
  - `UserId` (PK), `FullName`, `Email` (unique), `Phone`, `Address`, `Password`, `Role`, `JoiningDate`

- `Products`
  - `ProductId` (PK), `ProductName`, `Category`, `Brand`, `Model`, `Price`, `StockQuantity`, `MinimumStock`, `Description`

- `Cart`
  - `CartId` (PK), `UserId`, `ProductId`, `Quantity`, `AddedDate`

- `Bills`
  - `BillId` (PK), `CustomerName`, `CustomerPhone`, `TotalAmount`, `DiscountAmount`, `FinalAmount`, `DeliveryStatus` (NVARCHAR, default `Pending`), `BillDate`, `SalesmanId`

- `BillItems`
  - `BillItemId` (PK), `BillId` (FK), `ProductId`, `Quantity`, `UnitPrice`, `TotalPrice`

- `Reviews`
  - `ReviewId` (PK), `UserId`, `ProductId`, `Rating` (INT), `Comment`, `ReviewDate`

- `Offers`
  - `OfferId` (PK), `ProductId`, `DiscountPercent` (DECIMAL), `StartDate`, `EndDate`

## Database structure & connection

- Database initialization script: `sql/SuperShop_Init.sql` (creates DB, tables, and sample data).
- Default connection: LocalDB `(localdb)\MSSQLLocalDB` and database name `SuperShopDb`.
- The app looks for `dbconfig.json` in the application directory; if not found it falls back to the environment variable `SUPER_SHOP_CONNECTION`, and finally a built-in default LocalDB connection string.
- `AUTO_SETUP.bat` will create `dbconfig.json` with the detected instance and optionally set `SUPER_SHOP_CONNECTION`.

## How to run (developer / new machine)

1. Ensure .NET Desktop Runtime (8.x/10.x) and SQL Server LocalDB or SQL Express are installed.
2. From repository root run the auto-setup script (PowerShell/CMD):

```powershell
cd \path\to\supershop
.\AUTO_SETUP.bat
```

1. The script builds and publishes the app to `bin\Release\net8.0-windows\win-x64\publish` and creates `dbconfig.json`.
2. Launch the published executable `GroceryShop.exe` (or choose to launch when the script prompts).

## Sample accounts (created by the init script)

- Super Admin: <super@admin.com> / admin123
- Admin: <admin@store.com> / admin123
- Salesman: <sales@store.com> / sales123
- Customer: <customer@store.com> / cust123

## Developer notes

- The project uses `Microsoft.Data.SqlClient` for SQL Server access (check `GroceryShop.csproj`).
- Key helper functions live in `DatabaseHelper.cs` (DB init and helper methods for ratings, offers, and bills).
- UI entries to review, manage orders, and apply offers were added in `CustomerDashboard.cs`, `SalesmanDashboard.cs`, `AdminDashboard.cs`, `ReviewForm.cs`, and `ManageBillsForm.cs`.

## Troubleshooting

- If the script fails to connect to the database, verify LocalDB/SQL Express installation and permissions.
- If you already have a `SuperShopDb` and want to preserve data, choose `N` when the script asks to delete the DB.
- If you see package vulnerability warnings for `Microsoft.Data.SqlClient`, consider upgrading the package version in `GroceryShop.csproj`.

## Next suggestions (optional)

- Add an Admin UI to create and schedule `Offers` (currently the `Offers` table is present but no CRUD UI was added).
- Add more robust input validation and unit tests for DB helper methods.
- Export/Import data and backups for `SuperShopDb`.

---
Created and maintained for local small-store POS evaluation and testing.

## Architecture & data flow (high level)

- UI: Windows Forms provides separate dashboards for `Customer`, `Salesman`, `Admin`, and `Super Admin`.
- Data access: synchronous ADO.NET using `Microsoft.Data.SqlClient` from `DatabaseHelper.cs`.
- Checkout flow: cart -> create `Bills` row (computes discounts) -> insert `BillItems` -> update `Products.StockQuantity` -> clear `Cart`.
- Reviews: inserted into `Reviews` table; product lists show an average rating computed from `Reviews`.
- Offers (discounts): the app checks `Offers` for the product and current date/time; highest discount is applied per product when computing `DiscountAmount`.

## Files & important locations

- Root
  - `AUTO_SETUP.bat` — automated setup: DB init, build/publish, `dbconfig.json` creation, optional app launch.
  - `README.md` — this file.
  - `dbconfig.json` — generated by the auto-setup; custom connection string for the app.
- `sql/`
  - `SuperShop_Init.sql` — full DDL and sample data used by the auto-setup script.
- Source files (high level)
  - `DatabaseHelper.cs` — DB connection, initialization, helper methods (`GetAverageRating`, `GetActiveOfferPercent`, `InsertReview`, `UpdateBillDeliveryStatus`).
  - `CustomerDashboard.cs` — customer UI, product browsing, cart management with live discounts, and access to reviews and orders.
  - `SalesmanDashboard.cs` — point-of-sale UI for sales staff; applies offers at checkout and tracks handled orders.
  - `AdminDashboard.cs` — product management UI and a `Manage Orders` button.
  - `ReviewForm.cs` — review submission dialog.
  - `ViewReviewsForm.cs` — UI to view all customer reviews for a specific product.
  - `ManageBillsForm.cs` — admin order list and delivery status updater.
  - `ViewOrdersForm.cs` — dedicated UI for Customers and Salesmen to view their order history and update delivery tracking status.

## Example DDL snippets

The init script creates tables similar to the following simplified DDL (see `sql/SuperShop_Init.sql` for exact statements):

```sql
CREATE TABLE dbo.Products (
  ProductId INT IDENTITY(1,1) PRIMARY KEY,
  ProductName NVARCHAR(300) NOT NULL,
  Price DECIMAL(10,2) NOT NULL,
  StockQuantity INT NOT NULL,
  MinimumStock INT NOT NULL
);

CREATE TABLE dbo.Bills (
  BillId INT IDENTITY(1,1) PRIMARY KEY,
  CustomerName NVARCHAR(200),
  TotalAmount DECIMAL(10,2),
  DiscountAmount DECIMAL(10,2),
  FinalAmount DECIMAL(10,2),
  DeliveryStatus NVARCHAR(50) DEFAULT N'Pending',
  BillDate DATETIME2 DEFAULT SYSUTCDATETIME()
);
```

## Sample queries

- Get product average rating and active discount:

```sql
SELECT P.ProductId, P.ProductName,
  (SELECT AVG(CAST(Rating AS FLOAT)) FROM Reviews R WHERE R.ProductId = P.ProductId) AS AvgRating,
  (SELECT TOP 1 DiscountPercent FROM Offers O WHERE O.ProductId = P.ProductId AND @now BETWEEN O.StartDate AND O.EndDate ORDER BY DiscountPercent DESC) AS ActiveDiscount
FROM Products P;
```

- Mark a bill as shipped:

```sql
UPDATE Bills SET DeliveryStatus = 'Shipped' WHERE BillId = 123;
```

## How discounts are calculated (implementation notes)

- At checkout the app loops through cart items and queries `Offers` for each product and the current timestamp.
- If an active offer exists, the item discount is computed as `round(itemTotal * DiscountPercent / 100, 2)` and aggregated into `DiscountAmount` saved on the `Bills` row. `FinalAmount = TotalAmount - DiscountAmount`.

## How delivery status works

- The `Bills.DeliveryStatus` column stores a simple text status. The default is `Pending`.
- Admins can change the status from `Manage Orders` (UI) which calls `DatabaseHelper.UpdateBillDeliveryStatus(billId, status)`.

## Build & publish (manual)

Build and publish commands used by `AUTO_SETUP.bat`:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

To build without publishing:

```powershell
dotnet build -c Release
```

## Run & test (quick checklist)

- Run `AUTO_SETUP.bat` on a clean machine to initialize DB and publish the app.
- Launch `GroceryShop.exe` from the publish folder.
- Test flows:
  - Login as `customer@store.com`, browse products and submit a review.
  - Place orders as Customer and Salesman; confirm `Bills` entries show `DiscountAmount` and `FinalAmount`.
  - Login as Admin, open `Manage Orders` and update a bill's `DeliveryStatus`.

## Migrating an existing database (safe ALTER)

If you already have a `SuperShopDb` with an older schema, the safe approach is to ALTER tables rather than drop-and-recreate. Example to add `DeliveryStatus`:

```sql
ALTER TABLE dbo.Bills ADD DeliveryStatus NVARCHAR(50) DEFAULT N'Pending' NULL;
-- Optionally set existing rows to Pending where NULL
UPDATE dbo.Bills SET DeliveryStatus = N'Pending' WHERE DeliveryStatus IS NULL;
```

## Security & notes

- Passwords in the sample code are plain text only for demo convenience. For production, use secure password hashing (e.g., PBKDF2/Argon2) and TLS for DB connections.
- The project currently uses `Microsoft.Data.SqlClient` — check `GroceryShop.csproj` and update the package if security advisories are raised.

## Contribution & extension ideas

- Add Admin UI to create/update `Offers` and schedule them.
- Add order export / CSV / PDF receipt printing.
- Implement unit tests for `DatabaseHelper` methods and validation logic.

## License

This repository does not include a license file. Add a `LICENSE` if you plan to redistribute.

---
If you want, I can also:

- Add an `Offers` CRUD UI to the Admin dashboard.
- Create a small SQL migration script to ALTER existing databases instead of the current drop/re-create behavior in `AUTO_SETUP.bat`.
Just tell me which you prefer.
