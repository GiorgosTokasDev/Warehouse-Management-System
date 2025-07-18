# Warehouse Management System - Project Structure

```
WarehouseManagementSystem/
├── 📁 Database/
│   └── DatabaseManager.cs          # Database connection and operations
├── 📁 Forms/
│   ├── AddEditProductForm.cs       # Product add/edit dialog
│   ├── MainForm.cs                 # Main application window
│   └── StockTransactionForm.cs     # Stock transaction entry form
├── 📁 Managers/
│   ├── ProductManager.cs           # Product business logic
│   ├── ReportManager.cs            # Report generation and management
│   └── StockManager.cs             # Inventory and stock management
├── 📁 Models/
│   ├── Product.cs                  # Product data model
│   └── StockTransaction.cs         # Stock transaction data model
├── 📄 Application.cs               # Main application entry point
├── 📄 appsettings.json            # Application configuration
├── 📄 appsettings.example.json    # Example configuration template
└── 📄 secrets.json                # Sensitive configuration (gitignored)
```

## Project Components

### 🗂️ Database Layer
- **DatabaseManager.cs** - Handles database connections, queries, and data access operations

### 🖥️ User Interface Layer
- **MainForm.cs** - Primary application window with main navigation and display
- **AddEditProductForm.cs** - Dialog for adding new products or editing existing ones
- **StockTransactionForm.cs** - Form for recording stock movements and transactions

### 🔧 Business Logic Layer
- **ProductManager.cs** - Product-related business operations and validation
- **StockManager.cs** - Inventory management, stock level calculations
- **ReportManager.cs** - Report generation and data analysis functions

### 📊 Data Models
- **Product.cs** - Product entity with properties and validation
- **StockTransaction.cs** - Transaction record model for stock movements

### ⚙️ Configuration
- **Application.cs** - Main entry point and application startup
- **appsettings.json** - Application configuration and settings
- **secrets.json** - Sensitive data like connection strings (not in version control)