
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WarehouseManagementSystem.Database;
using WarehouseManagementSystem.Managers;
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Forms
{
    public class MainForm : Form
    {
        private DatabaseManager dbManager;
        private ProductManager productManager;
        private StockManager stockManager;
        private ReportManager reportManager;

        private DataGridView dgvInventory;

        public MainForm()
        {
            dbManager = new DatabaseManager();
            productManager = new ProductManager(dbManager);
            stockManager = new StockManager(dbManager);
            reportManager = new ReportManager(dbManager);

            InitializeComponent();
            LoadInventory();
        }

        private void InitializeComponent()
        {
            this.Text = "Warehouse Management System";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var menu = new MenuStrip();

            var productsMenu = new ToolStripMenuItem("Products");
            var mnuAddProduct = new ToolStripMenuItem("Add Product");
            var mnuEditProduct = new ToolStripMenuItem("Edit Selected");
            var mnuDeleteProduct = new ToolStripMenuItem("Delete Selected");
            mnuAddProduct.Click += MnuAddProduct_Click;
            mnuEditProduct.Click += MnuEditProduct_Click;
            mnuDeleteProduct.Click += MnuDeleteProduct_Click;
            productsMenu.DropDownItems.AddRange(new[] { mnuAddProduct, mnuEditProduct, mnuDeleteProduct });

            var stockMenu = new ToolStripMenuItem("Stock");
            var mnuStockIn = new ToolStripMenuItem("Stock In");
            var mnuStockOut = new ToolStripMenuItem("Stock Out");
            mnuStockIn.Click += (s, e) => HandleStockTransaction(StockTransactionType.In);
            mnuStockOut.Click += (s, e) => HandleStockTransaction(StockTransactionType.Out);
            stockMenu.DropDownItems.AddRange(new[] { mnuStockIn, mnuStockOut });

            var reportsMenu = new ToolStripMenuItem("Reports");
            var mnuInventory = new ToolStripMenuItem("Inventory Report");
            var mnuMovements = new ToolStripMenuItem("Stock Movement");
            var mnuLowStock = new ToolStripMenuItem("Low Stock");
            mnuInventory.Click += (s, e) => ShowReport(reportManager.GetInventoryReport(), "Inventory Report");
            mnuMovements.Click += (s, e) => ShowReport(reportManager.GetStockMovementReport(), "Stock Movements");
            mnuLowStock.Click += (s, e) => ShowReport(reportManager.GetLowStockReport(), "Low Stock Report");
            reportsMenu.DropDownItems.AddRange(new[] { mnuInventory, mnuMovements, mnuLowStock });

            menu.Items.AddRange(new[] { productsMenu, stockMenu, reportsMenu });
            this.MainMenuStrip = menu;
            this.Controls.Add(menu);

            dgvInventory = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                EnableHeadersVisualStyles = false,
                GridColor = Color.White,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.LightSteelBlue,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Segoe UI", 9)
                },
                RowTemplate = { Height = 28 }
            };

            this.Controls.Add(dgvInventory);

            var panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.White
            };

            var btnAdd = new Button
            {
                Text = "Add Product",
                BackColor = Color.DeepSkyBlue,
                ForeColor = Color.White,
                Location = new Point(10, 10),
                Size = new Size(100, 30)
            };
            btnAdd.Click += MnuAddProduct_Click;

            var btnEdit = new Button
            {
                Text = "Edit Product",
                BackColor = Color.MediumPurple,
                ForeColor = Color.White,
                Location = new Point(120, 10),
                Size = new Size(100, 30)
            };
            btnEdit.Click += MnuEditProduct_Click;

            var btnDelete = new Button
            {
                Text = "Delete Product",
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                Location = new Point(230, 10),
                Size = new Size(100, 30)
            };
            btnDelete.Click += MnuDeleteProduct_Click;

            panelBottom.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });
            this.Controls.Add(panelBottom);
        }

        private void LoadInventory()
        {
            dgvInventory.DataSource = stockManager.GetCurrentInventory();
        }

        private void MnuAddProduct_Click(object sender, EventArgs e)
        {
            using var form = new AddEditProductForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                productManager.AddProduct(form.Product);
                LoadInventory();
            }
        }

        private void MnuEditProduct_Click(object sender, EventArgs e)
        {
            if (dgvInventory.CurrentRow == null)
            {
                MessageBox.Show("Select a product to edit.");
                return;
            }

            var productCode = dgvInventory.CurrentRow.Cells["ProductCode"].Value.ToString();
            using var conn = dbManager.GetConnection();
            conn.Open();
            var cmd = new MySql.Data.MySqlClient.MySqlCommand(
                "SELECT ProductId FROM Products WHERE ProductCode = @code", conn);
            cmd.Parameters.AddWithValue("@code", productCode);
            var productId = Convert.ToInt32(cmd.ExecuteScalar());

            var product = productManager.GetProduct(productId);
            if (product == null)
            {
                MessageBox.Show("Product not found.");
                return;
            }

            using var form = new AddEditProductForm(product);
            if (form.ShowDialog() == DialogResult.OK)
            {
                productManager.UpdateProduct(form.Product);
                LoadInventory();
            }
        }

        private void MnuDeleteProduct_Click(object sender, EventArgs e)
        {
            if (dgvInventory.CurrentRow == null)
            {
                MessageBox.Show("Select a product to delete.");
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this product?",
                                          "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            var productCode = dgvInventory.CurrentRow.Cells["ProductCode"].Value.ToString();
            using var conn = dbManager.GetConnection();
            conn.Open();
            var cmd = new MySql.Data.MySqlClient.MySqlCommand(
                "SELECT ProductId FROM Products WHERE ProductCode = @code", conn);
            cmd.Parameters.AddWithValue("@code", productCode);
            var productId = Convert.ToInt32(cmd.ExecuteScalar());
            productManager.DeleteProduct(productId);
            LoadInventory();
        }

        private void HandleStockTransaction(StockTransactionType type)
        {
            using var form = new StockTransactionForm(type);
            if (form.ShowDialog() == DialogResult.OK)
            {
                stockManager.RecordStockTransaction(form.Transaction);
                LoadInventory();
            }
        }

        private void ShowReport(DataTable data, string title)
        {
            var form = new Form
            {
                Text = title,
                Size = new Size(800, 500),
                StartPosition = FormStartPosition.CenterParent,
                Font = new Font("Segoe UI", 9)
            };

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                DataSource = data,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.LightSteelBlue,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Segoe UI", 9)
                }
            };

            form.Controls.Add(grid);
            form.ShowDialog();
        }
    }
}

