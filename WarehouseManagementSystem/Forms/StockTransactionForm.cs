using WarehouseManagementSystem.Database;
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Forms
{
    public class StockTransactionForm : Form
    {
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Product Product { get; private set; }

       
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public StockTransaction Transaction { get; private set; }


        private DatabaseManager dbManager;
        private StockTransactionType transactionType;
        private ComboBox cmbProducts;
        private TextBox txtQuantity;
        private TextBox txtReference;
        private TextBox txtNotes;

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
            this.Size = new Size(420, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Font = new Font("Segoe UI", 9);

            var lblProduct = new Label { Text = "Product:", Location = new Point(20, 20), Size = new Size(100, 20) };
            cmbProducts = new ComboBox { Location = new Point(130, 18), Size = new Size(240, 24), DropDownStyle = ComboBoxStyle.DropDownList };

            var lblQuantity = new Label { Text = "Quantity:", Location = new Point(20, 60), Size = new Size(100, 20) };
            txtQuantity = new TextBox { Location = new Point(130, 58), Size = new Size(240, 24) };

            var lblReference = new Label { Text = "Reference:", Location = new Point(20, 100), Size = new Size(100, 20) };
            txtReference = new TextBox { Location = new Point(130, 98), Size = new Size(240, 24) };

            var lblNotes = new Label { Text = "Notes:", Location = new Point(20, 140), Size = new Size(100, 20) };
            txtNotes = new TextBox { Location = new Point(130, 138), Size = new Size(240, 60), Multiline = true };

            var btnSave = new Button
            {
                Text = "Save",
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                Location = new Point(200, 220),
                Size = new Size(80, 30)
            };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new Button
            {
                Text = "Cancel",
                BackColor = Color.LightGray,
                Location = new Point(290, 220),
                Size = new Size(80, 30)
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[]
            {
                lblProduct, cmbProducts,
                lblQuantity, txtQuantity,
                lblReference, txtReference,
                lblNotes, txtNotes,
                btnSave, btnCancel
            });
        }

        private void LoadProducts()
        {
            using var conn = dbManager.GetConnection();
            conn.Open();
            var sql = "SELECT ProductId, ProductName FROM Products ORDER BY ProductName";
            using var cmd = new MySql.Data.MySqlClient.MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                cmbProducts.Items.Add(new ComboBoxItem
                {
                    Text = reader.GetString("ProductName"),
                    Value = reader.GetInt32("ProductId")
                });
            }

            if (cmbProducts.Items.Count > 0)
                cmbProducts.SelectedIndex = 0;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbProducts.SelectedItem is not ComboBoxItem selectedItem)
            {
                MessageBox.Show("Please select a product!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Enter a valid quantity!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Transaction.ProductId = (int)selectedItem.Value;
            Transaction.Quantity = quantity;
            Transaction.Reference = txtReference.Text.Trim();
            Transaction.Notes = txtNotes.Text.Trim();

            this.DialogResult = DialogResult.OK;
        }

        private class ComboBoxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }
            public override string ToString() => Text;
        }
    }
}