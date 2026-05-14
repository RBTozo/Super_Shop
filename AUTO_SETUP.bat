@echo off
REM ============================================================================
REM                    SUPER SHOP - AUTO SETUP SCRIPT
REM          Grocery Shop Management System - SQL Server Edition
REM ============================================================================
REM This batch file automates the complete setup process on a new PC
REM ============================================================================

setlocal enabledelayedexpansion
cd /d "%~dp0"

REM Define colors for output
set "INFO=[INFO]"
set "SUCCESS=[SUCCESS]"
set "ERROR=[ERROR]"
set "WARNING=[WARNING]"

echo.
echo ============================================================================
echo                    SUPER SHOP AUTO SETUP
echo ============================================================================
echo.

REM ============================================================================
REM STEP 1: Check Prerequisites
REM ============================================================================
echo.
echo %INFO% Checking prerequisites...
echo.

REM Check .NET 8.0
echo %INFO% Checking .NET 8.0 Runtime...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo %ERROR% .NET 8.0 Runtime not found!
    echo.
    echo Please install .NET 8.0 Desktop Runtime from:
    echo https://dotnet.microsoft.com/en-us/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)
for /f "tokens=1" %%i in ('dotnet --version') do set DOTNET_VER=%%i
echo %SUCCESS% .NET %DOTNET_VER% found!

REM Check SQL Server availability
echo.
echo %INFO% Checking SQL Server...
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -Q "SELECT 1" >nul 2>&1
if errorlevel 1 (
    echo %WARNING% LocalDB not available, trying SQL Server Express...
    sqlcmd -S ".\SQLEXPRESS" -E -Q "SELECT 1" >nul 2>&1
    if errorlevel 1 (
        echo %ERROR% Neither LocalDB nor SQL Server Express found!
        echo.
        echo Please install one of:
        echo 1. SQL Server LocalDB:
        echo    https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb
        echo 2. SQL Server Express:
        echo    https://www.microsoft.com/en-us/sql-server/sql-server-editions-express
        echo.
        pause
        exit /b 1
    )
    set "SQL_INSTANCE=.\SQLEXPRESS"
) else (
    set "SQL_INSTANCE=(localdb)\MSSQLLocalDB"
)
echo %SUCCESS% SQL Server found: %SQL_INSTANCE%

REM ============================================================================
REM STEP 2: Detect SQL Server Instance
REM ============================================================================
echo.
echo %INFO% Detecting SQL Server instance...

REM Try LocalDB first
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -Q "SELECT 1" >nul 2>&1
if not errorlevel 1 (
    set "SQL_INSTANCE=(localdb)\MSSQLLocalDB"
    echo %SUCCESS% Using SQL Server LocalDB
    goto db_create
)

REM Try SQLEXPRESS
sqlcmd -S ".\SQLEXPRESS" -E -Q "SELECT 1" >nul 2>&1
if not errorlevel 1 (
    set "SQL_INSTANCE=.\SQLEXPRESS"
    echo %SUCCESS% Using SQL Server Express
    goto db_create
)

REM If here, no SQL Server found
echo %ERROR% Could not detect SQL Server instance!
pause
exit /b 1

REM ============================================================================
REM STEP 3: Create/Verify Database
REM ============================================================================
:db_create
echo.
echo %INFO% Setting up database...

REM Check if database exists using system tables
sqlcmd -S "%SQL_INSTANCE%" -E -Q "USE master; IF EXISTS(SELECT 1 FROM sys.databases WHERE name='SuperShopDb') SELECT 1" 2>nul | find "1" >nul 2>&1
if not errorlevel 1 (
    echo %WARNING% Database SuperShopDb already exists!
    echo.
    set /p DELETE_DB="Do you want to DELETE the existing database and create a new one? (Y/N): "
    if /i "!DELETE_DB!"=="Y" (
        echo %INFO% Dropping existing database...
        sqlcmd -S "%SQL_INSTANCE%" -E -Q "ALTER DATABASE SuperShopDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE SuperShopDb;" >nul 2>&1
        if errorlevel 1 (
            echo %ERROR% Failed to drop database!
            pause
            exit /b 1
        )
        echo %SUCCESS% Database dropped successfully!
        echo %INFO% Creating new database and tables...
        if exist "sql\SuperShop_Init.sql" (
            sqlcmd -S "%SQL_INSTANCE%" -E -i sql\SuperShop_Init.sql
            if errorlevel 1 (
                echo %ERROR% Failed to initialize database!
                pause
                exit /b 1
            )
            echo %SUCCESS% New database created successfully!
        ) else (
            echo %ERROR% sql\SuperShop_Init.sql not found!
            pause
            exit /b 1
        )
    ) else (
        echo %INFO% Keeping existing database, skipping creation...
    )
) else (
    echo %INFO% Creating new database and tables...
    if exist "sql\SuperShop_Init.sql" (
        sqlcmd -S "%SQL_INSTANCE%" -E -i sql\SuperShop_Init.sql
        if errorlevel 1 (
            echo %ERROR% Failed to initialize database!
            pause
            exit /b 1
        )
        echo %SUCCESS% Database created successfully!
    ) else (
        echo %ERROR% sql\SuperShop_Init.sql not found!
        pause
        exit /b 1
    )
)

REM ============================================================================
REM STEP 4: Create dbconfig.json
REM ============================================================================
echo.
echo %INFO% Creating database configuration file...

REM Determine output folder
set "OUTPUT_FOLDER=bin\Release\net8.0-windows\win-x64\publish"

if not exist "%OUTPUT_FOLDER%" (
    echo %WARNING% Publish folder not found, building application...
    dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained=false -o "%OUTPUT_FOLDER%"
    if errorlevel 1 (
        echo %ERROR% Failed to publish application!
        pause
        exit /b 1
    )
)

REM Create dbconfig.json with detected instance
echo %INFO% Creating dbconfig.json with connection string...

if "%SQL_INSTANCE%"=="(localdb)\MSSQLLocalDB" (
    set "CONN_STR=Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SuperShopDb;Integrated Security=True;TrustServerCertificate=True;"
) else (
    set "CONN_STR=Data Source=.\SQLEXPRESS;Initial Catalog=SuperShopDb;Integrated Security=True;TrustServerCertificate=True;"
)

(
    echo {
    echo   "ConnectionString": "%CONN_STR%"
    echo }
) > "dbconfig.json"

echo %SUCCESS% dbconfig.json created!

REM Also copy to publish folder if different
if not "%CD%"=="%OUTPUT_FOLDER%" (
    copy "dbconfig.json" "%OUTPUT_FOLDER%\dbconfig.json" >nul 2>&1
    echo %SUCCESS% dbconfig.json copied to publish folder!
)

REM ============================================================================
REM STEP 5: Set Environment Variable (Optional)
REM ============================================================================
echo.
echo %WARNING% Setting environment variable SUPER_SHOP_CONNECTION...
setx SUPER_SHOP_CONNECTION "%CONN_STR%"
set "SUPER_SHOP_CONNECTION=%CONN_STR%"
if errorlevel 1 (
    echo %WARNING% Could not set environment variable (may require admin privileges)
    echo You can manually set it or use dbconfig.json
) else (
    echo %SUCCESS% Environment variable set!
)

REM ============================================================================
REM STEP 6: Test Connection
REM ============================================================================
echo.
echo %INFO% Testing database connection...

sqlcmd -S "%SQL_INSTANCE%" -E -Q "SELECT 1" >nul 2>&1
if errorlevel 1 (
    echo %ERROR% Database connection failed!
    pause
    exit /b 1
)
echo %SUCCESS% Database connection successful!

REM ============================================================================
REM STEP 7: Display Setup Summary
REM ============================================================================
echo.
echo ============================================================================
echo                         SETUP COMPLETED!
echo ============================================================================
echo.
echo Setup Summary:
echo   SQL Server Instance: %SQL_INSTANCE%
echo   Database Name:       SuperShopDb
echo   Connection Config:   dbconfig.json
echo.
echo Test Credentials:
echo   Super Admin  - super@admin.com    / admin123
echo   Admin        - admin@store.com    / admin123
echo   Salesman     - sales@store.com    / sales123
echo   Customer     - customer@store.com / cust123
echo.
echo Application Location:
echo   Executable: %OUTPUT_FOLDER%\GroceryShop.exe
echo.
echo ============================================================================
echo.

REM ============================================================================
REM STEP 8: Offer to Launch Application
REM ============================================================================
echo.
set /p LAUNCH="Do you want to launch the application now? (Y/N): "
if /i "%LAUNCH%"=="Y" (
    echo %INFO% Launching application...
    if exist "%OUTPUT_FOLDER%\GroceryShop.exe" (
        start "" "%OUTPUT_FOLDER%\GroceryShop.exe"
        echo %SUCCESS% Application launched!
    ) else (
        echo %ERROR% GroceryShop.exe not found!
        pause
        exit /b 1
    )
) else (
    echo %INFO% Setup completed. You can run GroceryShop.exe manually.
    echo Location: %OUTPUT_FOLDER%\GroceryShop.exe
)

echo.
pause
exit /b 0

REM ============================================================================
REM END OF SETUP SCRIPT
REM ============================================================================
