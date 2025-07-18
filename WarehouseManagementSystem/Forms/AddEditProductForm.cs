
using System;
using System.Drawing;
using System.Windows.Forms;
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Forms
{
    public class AddEditProductForm : Form
    {
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Product Product { get; private set; }

        private bool isEditMode;

        private TextBox txtProductCode;
        private TextBox txtProductName;
        private TextBox txtDescription;
        private TextBox txtCategory;
        private TextBox txtUnitPrice;
        private TextBox txtMinStockLevel;

        public AddEditProductForm(Product product = null)
        {
            isEditMode = product != null;
            Product = product ?? new Product();
            InitializeComponent();

            if (isEditMode)
            {
                txtProductCode.Text = Product.ProductCode;
                txtProductName.Text = Product.ProductName;
                txtDescription.Text = Product.Description;
                txtCategory.Text = Product.Category;
                txtUnitPrice.Text = Product.UnitPrice.ToString("0.00");
                txtMinStockLevel.Text = Product.MinStockLevel.ToString();
            }
        }

        private void InitializeComponent()
        {
            this.Text = isEditMode ? "Edit Product" : "Add New Product";
            this.Size = new Size(420, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9);

            var labels = new[] { "Product Code", "Product Name", "Description", "Category", "Unit Price", "Min Stock Level" };
            TextBox[] fields = new TextBox[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                var lbl = new Label
                {
                    Text = labels[i] + ":",
                    Location = new Point(20, 25 + i * 45),
                    Size = new Size(110, 20)
                };

                var box = new TextBox
                {
                    Location = new Point(140, 25 + i * 45),
                    Size = new Size(240, labels[i] == "Description" ? 60 : 24),
                    Multiline = labels[i] == "Description"
                };
                fields[i] = box;
                this.Controls.Add(lbl);
                this.Controls.Add(box);
            }

            txtProductCode = fields[0];
            txtProductName = fields[1];
            txtDescription = fields[2];
            txtCategory = fields[3];
            txtUnitPrice = fields[4];
            txtMinStockLevel = fields[5];

            var btnSave = new Button
            {
                Text = "Save",
                BackColor = Color.MediumSeaGreen,
                ForeColor = Color.White,
                Location = new Point(200, 310),
                Size = new Size(80, 30)
            };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new Button
            {
                Text = "Cancel",
                BackColor = Color.White,
                Location = new Point(300, 310),
                Size = new Size(80, 30)
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductCode.Text) ||
                string.IsNullOrWhiteSpace(txtProductName.Text) ||
                !decimal.TryParse(txtUnitPrice.Text, out decimal price) ||
                !int.TryParse(txtMinStockLevel.Text, out int stock))
            {
                MessageBox.Show("Please fill all fields correctly.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Product.ProductCode = txtProductCode.Text.Trim();
            Product.ProductName = txtProductName.Text.Trim();
            Product.Description = txtDescription.Text.Trim();
            Product.Category = txtCategory.Text.Trim();
            Product.UnitPrice = price;
            Product.MinStockLevel = stock;

            this.DialogResult = DialogResult.OK;
        }
    }
}
