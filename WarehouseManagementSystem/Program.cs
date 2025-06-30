using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WarehouseManagementSystem
{
    // Product Model
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public decimal UnitPrice { get; set; }
        public int MinStockLevel { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    // Stock Transaction Model
    public class StockTransaction
    {
        public int TransactionId { get; set; }
        public int ProductId { get; set; }
        public StockTransactionType TransactionType { get; set; }
        public int Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Reference { get; set; }
        public string Notes { get; set; }
    }

    public enum StockTransactionType
    {
        In,
        Out
    }

    // Database Manager
    public class DatabaseManager
    {

        // Ensure you're using this exact format:
        private string connectionString =
            "Server=localhost;Database=warehouse_db;Uid=Georgios;Pwd=Cr3gn76!;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        public void InitializeDatabase()
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();

                    // Create Products table
                    var createProductsTable = @"
                        CREATE TABLE IF NOT EXISTS Products (
                            ProductId INT AUTO_INCREMENT PRIMARY KEY,
                            ProductCode VARCHAR(50) UNIQUE NOT NULL,
                            ProductName VARCHAR(255) NOT NULL,
                            Description TEXT,
                            Category VARCHAR(100),
                            UnitPrice DECIMAL(10,2),
                            MinStockLevel INT DEFAULT 0,
                            CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
                        )";

                    // Create Stock table
                    var createStockTable = @"
                        CREATE TABLE IF NOT EXISTS Stock (
                            StockId INT AUTO_INCREMENT PRIMARY KEY,
                            ProductId INT,
                            Quantity INT NOT NULL,
                            FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
                        )";

                    // Create StockTransactions table
                    var createTransactionsTable = @"
                        CREATE TABLE IF NOT EXISTS StockTransactions (
                            TransactionId INT AUTO_INCREMENT PRIMARY KEY,
                            ProductId INT,
                            TransactionType ENUM('IN', 'OUT') NOT NULL,
                            Quantity INT NOT NULL,
                            TransactionDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                            Reference VARCHAR(255),
                            Notes TEXT,
                            FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
                        )";

                    ExecuteCommand(createProductsTable, conn);
                    ExecuteCommand(createStockTable, conn);
                    ExecuteCommand(createTransactionsTable, conn);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Database initialization failed: {ex.Message}", "Database Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExecuteCommand(string sql, MySqlConnection conn)
        {
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }

    // Product Manager
    public class ProductManager
    {
        private DatabaseManager dbManager;

        public ProductManager(DatabaseManager dbManager)
        {
            this.dbManager = dbManager;
        }

        public DataTable GetAllProducts()
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = "SELECT * FROM Products ORDER BY ProductName";
                var adapter = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public Product GetProduct(int productId)
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = "SELECT * FROM Products WHERE ProductId = @ProductId";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductId", productId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Product
                            {
                                ProductId = reader.GetInt32("ProductId"),
                                ProductCode = reader.GetString("ProductCode"),
                                ProductName = reader.GetString("ProductName"),
                                Description = reader.IsDBNull("Description") ? "" : reader.GetString("Description"),
                                Category = reader.IsDBNull("Category") ? "" : reader.GetString("Category"),
                                UnitPrice = reader.GetDecimal("UnitPrice"),
                                MinStockLevel = reader.GetInt32("MinStockLevel"),
                                CreatedDate = reader.GetDateTime("CreatedDate")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public void AddProduct(Product product)
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = @"INSERT INTO Products (ProductCode, ProductName, Description, Category, UnitPrice, MinStockLevel) 
                           VALUES (@ProductCode, @ProductName, @Description, @Category, @UnitPrice, @MinStockLevel)";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductCode", product.ProductCode);
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@Description", product.Description);
                    cmd.Parameters.AddWithValue("@Category", product.Category);
                    cmd.Parameters.AddWithValue("@UnitPrice", product.UnitPrice);
                    cmd.Parameters.AddWithValue("@MinStockLevel", product.MinStockLevel);
                    cmd.ExecuteNonQuery();
                    var productId = (int)cmd.LastInsertedId;

                    // Initialize stock record
                    var stockSql = "INSERT INTO Stock (ProductId, Quantity) VALUES (@ProductId, 0)";
                    using (var stockCmd = new MySqlCommand(stockSql, conn))
                    {
                        stockCmd.Parameters.AddWithValue("@ProductId", productId);
                        stockCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void UpdateProduct(Product product)
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = @"UPDATE Products SET ProductCode = @ProductCode, ProductName = @ProductName, 
                           Description = @Description, Category = @Category, UnitPrice = @UnitPrice, 
                           MinStockLevel = @MinStockLevel WHERE ProductId = @ProductId";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductId", product.ProductId);
                    cmd.Parameters.AddWithValue("@ProductCode", product.ProductCode);
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@Description", product.Description);
                    cmd.Parameters.AddWithValue("@Category", product.Category);
                    cmd.Parameters.AddWithValue("@UnitPrice", product.UnitPrice);
                    cmd.Parameters.AddWithValue("@MinStockLevel", product.MinStockLevel);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteProduct(int productId)
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Delete stock record
                        var deleteStock = "DELETE FROM Stock WHERE ProductId = @ProductId";
                        using (var cmd = new MySqlCommand(deleteStock, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductId", productId);
                            cmd.ExecuteNonQuery();
                        }

                        // Delete transactions
                        var deleteTransactions = "DELETE FROM StockTransactions WHERE ProductId = @ProductId";
                        using (var cmd = new MySqlCommand(deleteTransactions, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductId", productId);
                            cmd.ExecuteNonQuery();
                        }

                        // Delete product
                        var deleteProduct = "DELETE FROM Products WHERE ProductId = @ProductId";
                        using (var cmd = new MySqlCommand(deleteProduct, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductId", productId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }

    // Stock Manager
    public class StockManager
    {
        private DatabaseManager dbManager;

        public StockManager(DatabaseManager dbManager)
        {
            this.dbManager = dbManager;
        }

        public void RecordStockTransaction(StockTransaction transaction)
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                using (var dbTransaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Insert transaction record
                        var sql = @"INSERT INTO StockTransactions (ProductId, TransactionType, Quantity, Reference, Notes) 
                                   VALUES (@ProductId, @TransactionType, @Quantity, @Reference, @Notes)";

                        using (var cmd = new MySqlCommand(sql, conn, dbTransaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductId", transaction.ProductId);
                            cmd.Parameters.AddWithValue("@TransactionType", transaction.TransactionType.ToString().ToUpper());
                            cmd.Parameters.AddWithValue("@Quantity", transaction.Quantity);
                            cmd.Parameters.AddWithValue("@Reference", transaction.Reference ?? "");
                            cmd.Parameters.AddWithValue("@Notes", transaction.Notes ?? "");
                            cmd.ExecuteNonQuery();
                        }

                        // Update stock quantity
                        var quantityChange = transaction.TransactionType == StockTransactionType.In
                            ? transaction.Quantity : -transaction.Quantity;

                        var updateStock = "UPDATE Stock SET Quantity = Quantity + @QuantityChange WHERE ProductId = @ProductId";
                        using (var cmd = new MySqlCommand(updateStock, conn, dbTransaction))
                        {
                            cmd.Parameters.AddWithValue("@QuantityChange", quantityChange);
                            cmd.Parameters.AddWithValue("@ProductId", transaction.ProductId);
                            cmd.ExecuteNonQuery();
                        }

                        dbTransaction.Commit();
                    }
                    catch
                    {
                        dbTransaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public DataTable GetStockTransactions()
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT st.TransactionId, p.ProductCode, p.ProductName, st.TransactionType, 
                           st.Quantity, st.TransactionDate, st.Reference, st.Notes
                           FROM StockTransactions st
                           JOIN Products p ON st.ProductId = p.ProductId
                           ORDER BY st.TransactionDate DESC";

                var adapter = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public DataTable GetCurrentInventory()
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT p.ProductCode, p.ProductName, p.Category, s.Quantity, 
                           p.MinStockLevel, p.UnitPrice,
                           CASE WHEN s.Quantity <= p.MinStockLevel THEN 'Low Stock' ELSE 'OK' END as Status
                           FROM Products p
                           JOIN Stock s ON p.ProductId = s.ProductId
                           ORDER BY p.ProductName";

                var adapter = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }
    }

    // Report Manager
    public class ReportManager
    {
        private DatabaseManager dbManager;

        public ReportManager(DatabaseManager dbManager)
        {
            this.dbManager = dbManager;
        }

        public DataTable GetInventoryReport()
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT p.ProductCode, p.ProductName, p.Category, s.Quantity, 
                           p.UnitPrice, (s.Quantity * p.UnitPrice) as TotalValue,
                           p.MinStockLevel,
                           CASE WHEN s.Quantity <= p.MinStockLevel THEN 'Low Stock' ELSE 'Adequate' END as StockStatus
                           FROM Products p
                           JOIN Stock s ON p.ProductId = s.ProductId
                           ORDER BY TotalValue DESC";

                var adapter = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public DataTable GetStockMovementReport()
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT DATE(st.TransactionDate) as Date, p.ProductCode, p.ProductName,
                           SUM(CASE WHEN st.TransactionType = 'IN' THEN st.Quantity ELSE 0 END) as StockIn,
                           SUM(CASE WHEN st.TransactionType = 'OUT' THEN st.Quantity ELSE 0 END) as StockOut,
                           SUM(CASE WHEN st.TransactionType = 'IN' THEN st.Quantity ELSE -st.Quantity END) as NetMovement
                           FROM StockTransactions st
                           JOIN Products p ON st.ProductId = p.ProductId
                           WHERE st.TransactionDate >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
                           GROUP BY DATE(st.TransactionDate), p.ProductId
                           ORDER BY Date DESC, p.ProductName";

                var adapter = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public DataTable GetLowStockReport()
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT p.ProductCode, p.ProductName, p.Category, s.Quantity, 
                           p.MinStockLevel, (p.MinStockLevel - s.Quantity) as ShortfallQuantity,
                           p.UnitPrice, ((p.MinStockLevel - s.Quantity) * p.UnitPrice) as ReorderValue
                           FROM Products p
                           JOIN Stock s ON p.ProductId = s.ProductId
                           WHERE s.Quantity <= p.MinStockLevel
                           ORDER BY ShortfallQuantity DESC";

                var adapter = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }
    }

    // Add/Edit Product Form
    public class AddEditProductForm : Form
    {
        public Product Product { get; private set; }
        private bool isEditMode;

        public AddEditProductForm(Product product = null)
        {
            InitializeComponent();

            if (product != null)
            {
                isEditMode = true;
                Product = product;
                PopulateFields();
                this.Text = "Edit Product";
            }
            else
            {
                isEditMode = false;
                Product = new Product();
                this.Text = "Add New Product";
            }
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Create controls
            var lblCode = new Label { Text = "Product Code:", Location = new Point(20, 20), Size = new Size(100, 20) };
            var txtCode = new TextBox { Name = "txtCode", Location = new Point(130, 18), Size = new Size(220, 20) };

            var lblName = new Label { Text = "Product Name:", Location = new Point(20, 50), Size = new Size(100, 20) };
            var txtName = new TextBox { Name = "txtName", Location = new Point(130, 48), Size = new Size(220, 20) };

            var lblDescription = new Label { Text = "Description:", Location = new Point(20, 80), Size = new Size(100, 20) };
            var txtDescription = new TextBox { Name = "txtDescription", Location = new Point(130, 78), Size = new Size(220, 60), Multiline = true };

            var lblCategory = new Label { Text = "Category:", Location = new Point(20, 150), Size = new Size(100, 20) };
            var txtCategory = new TextBox { Name = "txtCategory", Location = new Point(130, 148), Size = new Size(220, 20) };

            var lblPrice = new Label { Text = "Unit Price:", Location = new Point(20, 180), Size = new Size(100, 20) };
            var txtPrice = new TextBox { Name = "txtPrice", Location = new Point(130, 178), Size = new Size(220, 20) };

            var lblMinStock = new Label { Text = "Min Stock Level:", Location = new Point(20, 210), Size = new Size(100, 20) };
            var txtMinStock = new TextBox { Name = "txtMinStock", Location = new Point(130, 208), Size = new Size(220, 20) };

            var btnSave = new Button { Text = "Save", Location = new Point(180, 260), Size = new Size(80, 30) };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new Button { Text = "Cancel", Location = new Point(270, 260), Size = new Size(80, 30) };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] {
                lblCode, txtCode, lblName, txtName, lblDescription, txtDescription,
                lblCategory, txtCategory, lblPrice, txtPrice, lblMinStock, txtMinStock,
                btnSave, btnCancel
            });
        }

        private void PopulateFields()
        {
            var txtCode = this.Controls.Find("txtCode", false)[0] as TextBox;
            var txtName = this.Controls.Find("txtName", false)[0] as TextBox;
            var txtDescription = this.Controls.Find("txtDescription", false)[0] as TextBox;
            var txtCategory = this.Controls.Find("txtCategory", false)[0] as TextBox;
            var txtPrice = this.Controls.Find("txtPrice", false)[0] as TextBox;
            var txtMinStock = this.Controls.Find("txtMinStock", false)[0] as TextBox;

            txtCode.Text = Product.ProductCode;
            txtName.Text = Product.ProductName;
            txtDescription.Text = Product.Description;
            txtCategory.Text = Product.Category;
            txtPrice.Text = Product.UnitPrice.ToString("F2");
            txtMinStock.Text = Product.MinStockLevel.ToString();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var txtCode = this.Controls.Find("txtCode", false)[0] as TextBox;
            var txtName = this.Controls.Find("txtName", false)[0] as TextBox;
            var txtDescription = this.Controls.Find("txtDescription", false)[0] as TextBox;
            var txtCategory = this.Controls.Find("txtCategory", false)[0] as TextBox;
            var txtPrice = this.Controls.Find("txtPrice", false)[0] as TextBox;
            var txtMinStock = this.Controls.Find("txtMinStock", false)[0] as TextBox;

            // Validation
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Product Code and Name are required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Please enter a valid price!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtMinStock.Text, out int minStock) || minStock < 0)
            {
                MessageBox.Show("Please enter a valid minimum stock level!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Update product object
            Product.ProductCode = txtCode.Text.Trim();
            Product.ProductName = txtName.Text.Trim();
            Product.Description = txtDescription.Text.Trim();
            Product.Category = txtCategory.Text.Trim();
            Product.UnitPrice = price;
            Product.MinStockLevel = minStock;

            this.DialogResult = DialogResult.OK;
        }

        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Stock Transaction Form
    public class StockTransactionForm : Form
    {
        public StockTransaction Transaction { get; private set; }
        private DatabaseManager dbManager;
        private StockTransactionType transactionType;

        public StockTransactionForm(StockTransactionType type)
        {
            transactionType = type;
            Transaction = new StockTransaction { TransactionType = type };
            dbManager = new DatabaseManager();
            InitializeComponent();
            LoadProducts();

            this.Text = type == StockTransactionType.In ? "Stock In" : "Stock Out";
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var lblProduct = new Label { Text = "Product:", Location = new Point(20, 20), Size = new Size(80, 20) };
            var cmbProduct = new ComboBox { Name = "cmbProduct", Location = new Point(110, 18), Size = new Size(250, 20), DropDownStyle = ComboBoxStyle.DropDownList };

            var lblQuantity = new Label { Text = "Quantity:", Location = new Point(20, 60), Size = new Size(80, 20) };
            var txtQuantity = new TextBox { Name = "txtQuantity", Location = new Point(110, 58), Size = new Size(150, 20) };

            var lblReference = new Label { Text = "Reference:", Location = new Point(20, 100), Size = new Size(80, 20) };
            var txtReference = new TextBox { Name = "txtReference", Location = new Point(110, 98), Size = new Size(250, 20) };

            var lblNotes = new Label { Text = "Notes:", Location = new Point(20, 140), Size = new Size(80, 20) };
            var txtNotes = new TextBox { Name = "txtNotes", Location = new Point(110, 138), Size = new Size(250, 60), Multiline = true };

            var btnSave = new Button { Text = "Save", Location = new Point(180, 220), Size = new Size(80, 30) };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new Button { Text = "Cancel", Location = new Point(270, 220), Size = new Size(80, 30) };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] {
                lblProduct, cmbProduct, lblQuantity, txtQuantity, lblReference, txtReference,
                lblNotes, txtNotes, btnSave, btnCancel
            });
        }

        private void LoadProducts()
        {
            var cmbProduct = this.Controls.Find("cmbProduct", false)[0] as ComboBox;

            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = "SELECT ProductId, CONCAT(ProductCode, ' - ', ProductName) as DisplayText FROM Products ORDER BY ProductName";
                var adapter = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                adapter.Fill(dt);

                cmbProduct.DisplayMember = "DisplayText";
                cmbProduct.ValueMember = "ProductId";
                cmbProduct.DataSource = dt;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var cmbProduct = this.Controls.Find("cmbProduct", false)[0] as ComboBox;
            var txtQuantity = this.Controls.Find("txtQuantity", false)[0] as TextBox;
            var txtReference = this.Controls.Find("txtReference", false)[0] as TextBox;
            var txtNotes = this.Controls.Find("txtNotes", false)[0] as TextBox;

            // Validation
            if (cmbProduct.SelectedValue == null)
            {
                MessageBox.Show("Please select a product!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if sufficient stock exists for stock out
            if (transactionType == StockTransactionType.Out)
            {
                var currentStock = GetCurrentStock(Convert.ToInt32(cmbProduct.SelectedValue));
                if (currentStock < quantity)
                {
                    MessageBox.Show($"Insufficient stock! Available: {currentStock}, Requested: {quantity}",
                        "Stock Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Create transaction
            Transaction.ProductId = Convert.ToInt32(cmbProduct.SelectedValue);
            Transaction.Quantity = quantity;
            Transaction.Reference = txtReference.Text.Trim();
            Transaction.Notes = txtNotes.Text.Trim();
            Transaction.TransactionDate = DateTime.Now;

            this.DialogResult = DialogResult.OK;
        }

        private int GetCurrentStock(int productId)
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = "SELECT Quantity FROM Stock WHERE ProductId = @ProductId";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductId", productId);
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Main Form
    public class MainForm : Form
    {
        private DatabaseManager dbManager;
        private ProductManager productManager;
        private StockManager stockManager;
        private ReportManager reportManager;

        public MainForm()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            productManager = new ProductManager(dbManager);
            stockManager = new StockManager(dbManager);
            reportManager = new ReportManager(dbManager);

            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Size = new Size(1000, 700);
            this.Text = "Warehouse Management System";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 245, 249);

            // Create TabControl
            var tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Segoe UI", 10);

            // Products Tab
            var productsTab = new TabPage("Products");
            CreateProductsTab(productsTab);
            tabControl.TabPages.Add(productsTab);

            // Stock Tab
            var stockTab = new TabPage("Stock Management");
            CreateStockTab(stockTab);
            tabControl.TabPages.Add(stockTab);

            // Reports Tab
            var reportsTab = new TabPage("Reports");
            CreateReportsTab(reportsTab);
            tabControl.TabPages.Add(reportsTab);

            // Status Bar
            var statusStrip = new StatusStrip();
            statusStrip.Items.Add(new ToolStripStatusLabel
            {
                Text = "Ready",
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            });
            statusStrip.Items.Add(new ToolStripStatusLabel
            {
                Text = "Database: warehouse_db",
                TextAlign = ContentAlignment.MiddleRight
            });
            statusStrip.Dock = DockStyle.Bottom;

            this.Controls.Add(tabControl);
            this.Controls.Add(statusStrip);
        }

        private void CreateProductsTab(TabPage tab)
        {
            tab.BackColor = Color.White;

            // Product DataGridView
            var dgvProducts = new DataGridView();
            dgvProducts.Name = "dgvProducts";
            dgvProducts.Dock = DockStyle.Fill;
            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProducts.BackgroundColor = Color.White;
            dgvProducts.BorderStyle = BorderStyle.None;
            dgvProducts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 245, 249);
            dgvProducts.Font = new Font("Segoe UI", 9);

            // Panel for buttons
            var buttonPanel = new Panel();
            buttonPanel.Height = 60;
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.BackColor = Color.FromArgb(240, 245, 249);
            buttonPanel.Padding = new Padding(10);

            var btnAddProduct = new Button();
            btnAddProduct.Text = "Add Product";
            btnAddProduct.Size = new Size(120, 35);
            btnAddProduct.Location = new Point(10, 10);
            btnAddProduct.BackColor = Color.FromArgb(52, 152, 219);
            btnAddProduct.ForeColor = Color.White;
            btnAddProduct.FlatStyle = FlatStyle.Flat;
            btnAddProduct.FlatAppearance.BorderSize = 0;
            btnAddProduct.Click += BtnAddProduct_Click;

            var btnEditProduct = new Button();
            btnEditProduct.Text = "Edit Product";
            btnEditProduct.Size = new Size(120, 35);
            btnEditProduct.Location = new Point(140, 10);
            btnEditProduct.BackColor = Color.FromArgb(155, 89, 182);
            btnEditProduct.ForeColor = Color.White;
            btnEditProduct.FlatStyle = FlatStyle.Flat;
            btnEditProduct.FlatAppearance.BorderSize = 0;
            btnEditProduct.Click += BtnEditProduct_Click;

            var btnDeleteProduct = new Button();
            btnDeleteProduct.Text = "Delete Product";
            btnDeleteProduct.Size = new Size(120, 35);
            btnDeleteProduct.Location = new Point(270, 10);
            btnDeleteProduct.BackColor = Color.FromArgb(231, 76, 60);
            btnDeleteProduct.ForeColor = Color.White;
            btnDeleteProduct.FlatStyle = FlatStyle.Flat;
            btnDeleteProduct.FlatAppearance.BorderSize = 0;
            btnDeleteProduct.Click += BtnDeleteProduct_Click;

            buttonPanel.Controls.AddRange(new Control[] { btnAddProduct, btnEditProduct, btnDeleteProduct });

            tab.Controls.Add(dgvProducts);
            tab.Controls.Add(buttonPanel);
        }

        private void CreateStockTab(TabPage tab)
        {
            tab.BackColor = Color.White;

            var splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Orientation = Orientation.Horizontal;
            splitContainer.SplitterDistance = 300;
            splitContainer.BackColor = Color.White;

            // Top panel - Stock Movements
            var topPanel = new Panel();
            topPanel.Dock = DockStyle.Fill;
            topPanel.Padding = new Padding(5);

            var dgvStock = new DataGridView();
            dgvStock.Name = "dgvStock";
            dgvStock.Dock = DockStyle.Fill;
            dgvStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvStock.BackgroundColor = Color.White;
            dgvStock.BorderStyle = BorderStyle.None;
            dgvStock.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 245, 249);
            dgvStock.Font = new Font("Segoe UI", 9);

            var stockButtonPanel = new Panel();
            stockButtonPanel.Height = 60;
            stockButtonPanel.Dock = DockStyle.Bottom;
            stockButtonPanel.BackColor = Color.FromArgb(240, 245, 249);
            stockButtonPanel.Padding = new Padding(10);

            var btnStockIn = new Button();
            btnStockIn.Text = "Stock In";
            btnStockIn.Size = new Size(120, 35);
            btnStockIn.Location = new Point(10, 10);
            btnStockIn.BackColor = Color.FromArgb(46, 204, 113);
            btnStockIn.ForeColor = Color.White;
            btnStockIn.FlatStyle = FlatStyle.Flat;
            btnStockIn.FlatAppearance.BorderSize = 0;
            btnStockIn.Click += BtnStockIn_Click;

            var btnStockOut = new Button();
            btnStockOut.Text = "Stock Out";
            btnStockOut.Size = new Size(120, 35);
            btnStockOut.Location = new Point(140, 10);
            btnStockOut.BackColor = Color.FromArgb(230, 126, 34);
            btnStockOut.ForeColor = Color.White;
            btnStockOut.FlatStyle = FlatStyle.Flat;
            btnStockOut.FlatAppearance.BorderSize = 0;
            btnStockOut.Click += BtnStockOut_Click;

            stockButtonPanel.Controls.AddRange(new Control[] { btnStockIn, btnStockOut });

            topPanel.Controls.Add(dgvStock);
            topPanel.Controls.Add(stockButtonPanel);

            // Bottom panel - Current Inventory
            var bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Fill;
            bottomPanel.Padding = new Padding(5);

            var dgvInventory = new DataGridView();
            dgvInventory.Name = "dgvInventory";
            dgvInventory.Dock = DockStyle.Fill;
            dgvInventory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvInventory.BackgroundColor = Color.White;
            dgvInventory.BorderStyle = BorderStyle.None;
            dgvInventory.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 245, 249);
            dgvInventory.Font = new Font("Segoe UI", 9);

            var lblInventory = new Label();
            lblInventory.Text = "  Current Inventory Levels";
            lblInventory.Dock = DockStyle.Top;
            lblInventory.Height = 30;
            lblInventory.TextAlign = ContentAlignment.MiddleLeft;
            lblInventory.BackColor = Color.FromArgb(52, 152, 219);
            lblInventory.ForeColor = Color.White;
            lblInventory.Font = new Font(lblInventory.Font, FontStyle.Bold);

            bottomPanel.Controls.Add(dgvInventory);
            bottomPanel.Controls.Add(lblInventory);

            splitContainer.Panel1.Controls.Add(topPanel);
            splitContainer.Panel2.Controls.Add(bottomPanel);

            tab.Controls.Add(splitContainer);
        }

        private void CreateReportsTab(TabPage tab)
        {
            tab.BackColor = Color.White;

            var reportsPanel = new Panel();
            reportsPanel.Dock = DockStyle.Fill;
            reportsPanel.Padding = new Padding(20);
            reportsPanel.BackColor = Color.White;

            var reportButtonsPanel = new Panel();
            reportButtonsPanel.Dock = DockStyle.Top;
            reportButtonsPanel.Height = 70;
            reportButtonsPanel.BackColor = Color.FromArgb(240, 245, 249);
            reportButtonsPanel.Padding = new Padding(10);

            var btnInventoryReport = new Button();
            btnInventoryReport.Text = "Inventory Report";
            btnInventoryReport.Size = new Size(160, 40);
            btnInventoryReport.Location = new Point(10, 15);
            btnInventoryReport.BackColor = Color.FromArgb(52, 152, 219);
            btnInventoryReport.ForeColor = Color.White;
            btnInventoryReport.FlatStyle = FlatStyle.Flat;
            btnInventoryReport.FlatAppearance.BorderSize = 0;
            btnInventoryReport.Click += BtnInventoryReport_Click;

            var btnStockMovementReport = new Button();
            btnStockMovementReport.Text = "Stock Movement";
            btnStockMovementReport.Size = new Size(160, 40);
            btnStockMovementReport.Location = new Point(180, 15);
            btnStockMovementReport.BackColor = Color.FromArgb(155, 89, 182);
            btnStockMovementReport.ForeColor = Color.White;
            btnStockMovementReport.FlatStyle = FlatStyle.Flat;
            btnStockMovementReport.FlatAppearance.BorderSize = 0;
            btnStockMovementReport.Click += BtnStockMovementReport_Click;

            var btnLowStockReport = new Button();
            btnLowStockReport.Text = "Low Stock Alert";
            btnLowStockReport.Size = new Size(160, 40);
            btnLowStockReport.Location = new Point(350, 15);
            btnLowStockReport.BackColor = Color.FromArgb(231, 76, 60);
            btnLowStockReport.ForeColor = Color.White;
            btnLowStockReport.FlatStyle = FlatStyle.Flat;
            btnLowStockReport.FlatAppearance.BorderSize = 0;
            btnLowStockReport.Click += BtnLowStockReport_Click;

            reportButtonsPanel.Controls.AddRange(new Control[] {
                btnInventoryReport, btnStockMovementReport, btnLowStockReport
            });

            var dgvReports = new DataGridView();
            dgvReports.Name = "dgvReports";
            dgvReports.Dock = DockStyle.Fill;
            dgvReports.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReports.BackgroundColor = Color.White;
            dgvReports.BorderStyle = BorderStyle.None;
            dgvReports.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 245, 249);
            dgvReports.Font = new Font("Segoe UI", 9);

            reportsPanel.Controls.Add(dgvReports);
            reportsPanel.Controls.Add(reportButtonsPanel);

            tab.Controls.Add(reportsPanel);
        }

        // Event Handlers
        private void BtnAddProduct_Click(object sender, EventArgs e)
        {
            var addProductForm = new AddEditProductForm();
            if (addProductForm.ShowDialog() == DialogResult.OK)
            {
                productManager.AddProduct(addProductForm.Product);
                LoadProducts();
            }
        }

        private void BtnEditProduct_Click(object sender, EventArgs e)
        {
            var dgv = this.Controls.Find("dgvProducts", true)[0] as DataGridView;
            if (dgv.SelectedRows.Count > 0)
            {
                var productId = Convert.ToInt32(dgv.SelectedRows[0].Cells["ProductId"].Value);
                var product = productManager.GetProduct(productId);

                var editForm = new AddEditProductForm(product);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    productManager.UpdateProduct(editForm.Product);
                    LoadProducts();
                }
            }
        }

        private void BtnDeleteProduct_Click(object sender, EventArgs e)
        {
            var dgv = this.Controls.Find("dgvProducts", true)[0] as DataGridView;
            if (dgv.SelectedRows.Count > 0)
            {
                var productId = Convert.ToInt32(dgv.SelectedRows[0].Cells["ProductId"].Value);
                if (MessageBox.Show("Are you sure you want to delete this product?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    productManager.DeleteProduct(productId);
                    LoadProducts();
                }
            }
        }

        private void BtnStockIn_Click(object sender, EventArgs e)
        {
            var stockForm = new StockTransactionForm(StockTransactionType.In);
            if (stockForm.ShowDialog() == DialogResult.OK)
            {
                stockManager.RecordStockTransaction(stockForm.Transaction);
                LoadStockData();
            }
        }

        private void BtnStockOut_Click(object sender, EventArgs e)
        {
            var stockForm = new StockTransactionForm(StockTransactionType.Out);
            if (stockForm.ShowDialog() == DialogResult.OK)
            {
                stockManager.RecordStockTransaction(stockForm.Transaction);
                LoadStockData();
            }
        }

        private void BtnInventoryReport_Click(object sender, EventArgs e)
        {
            var dgv = this.Controls.Find("dgvReports", true)[0] as DataGridView;
            dgv.DataSource = reportManager.GetInventoryReport();
        }

        private void BtnStockMovementReport_Click(object sender, EventArgs e)
        {
            var dgv = this.Controls.Find("dgvReports", true)[0] as DataGridView;
            dgv.DataSource = reportManager.GetStockMovementReport();
        }

        private void BtnLowStockReport_Click(object sender, EventArgs e)
        {
            var dgv = this.Controls.Find("dgvReports", true)[0] as DataGridView;
            dgv.DataSource = reportManager.GetLowStockReport();
        }

        private void LoadProducts()
        {
            var dgv = this.Controls.Find("dgvProducts", true)[0] as DataGridView;
            dgv.DataSource = productManager.GetAllProducts();
        }

        private void LoadStockData()
        {
            var dgvStock = this.Controls.Find("dgvStock", true)[0] as DataGridView;
            var dgvInventory = this.Controls.Find("dgvInventory", true)[0] as DataGridView;

            dgvStock.DataSource = stockManager.GetStockTransactions();
            dgvInventory.DataSource = stockManager.GetCurrentInventory();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadStockData();
        }

        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Program Entry Point
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize database
            var dbManager = new DatabaseManager();
            try
            {
                dbManager.InitializeDatabase();
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}\n\nPlease ensure MySQL is running and accessible.",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}