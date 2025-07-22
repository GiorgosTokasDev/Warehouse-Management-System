
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

        
private const int FORM_WIDTH = 420;
private const int FORM_HEIGHT = 400;
private const int FONT_SIZE = 9;

private const int LEFT_MARGIN = 20;
private const int TOP_MARGIN = 25;
private const int VERTICAL_SPACING = 45;

private const int LABEL_WIDTH = 110;
private const int LABEL_HEIGHT = 20;

private const int TEXTBOX_LEFT = 140;
private const int TEXTBOX_WIDTH = 240;
private const int TEXTBOX_HEIGHT = 24;
private const int MULTILINE_TEXTBOX_HEIGHT = 60;

private const int BUTTON_TOP = 310;
private const int BUTTON_WIDTH = 80;
private const int BUTTON_HEIGHT = 30;
private const int SAVE_BUTTON_LEFT = 200;
private const int CANCEL_BUTTON_LEFT = 300;

private void InitializeComponent()
{
    this.Text = isEditMode ? "Edit Product" : "Add New Product";
    this.Size = new Size(FORM_WIDTH, 450); // Αύξηση ύψους για καλύτερο layout
    this.StartPosition = FormStartPosition.CenterParent;
    this.FormBorderStyle = FormBorderStyle.FixedDialog;
    this.MaximizeBox = false;
    this.Font = new Font("Segoe UI", FONT_SIZE);

    var labels = new[] { "Product Code", "Product Name", "Description", "Category", "Unit Price", "Min Stock Level" };
    TextBox[] fields = new TextBox[labels.Length];
    
    int currentTop = TOP_MARGIN;
    const int MULTILINE_EXTRA_SPACING = 40; // Επιπλέον spacing μετά το multiline
    
    for (int i = 0; i < labels.Length; i++)
    {
        var lbl = new Label
        {
            Text = labels[i] + ":",
            Location = new Point(LEFT_MARGIN, currentTop),
            Size = new Size(LABEL_WIDTH, LABEL_HEIGHT)
        };

        bool isDescription = labels[i] == "Description";
        var box = new TextBox
        {
            Location = new Point(TEXTBOX_LEFT, currentTop),
            Size = new Size(TEXTBOX_WIDTH, isDescription ? MULTILINE_TEXTBOX_HEIGHT : TEXTBOX_HEIGHT),
            Multiline = isDescription
        };
        
        fields[i] = box;
        this.Controls.Add(lbl);
        this.Controls.Add(box);
        
        // Υπολογισμός επόμενης θέσης
        if (isDescription)
        {
            currentTop += VERTICAL_SPACING + MULTILINE_EXTRA_SPACING; // Επιπλέον χώρος μετά το Description
        }
        else
        {
            currentTop += VERTICAL_SPACING;
        }
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
        Location = new Point(SAVE_BUTTON_LEFT, 360), // Μετακίνηση κουμπιών χαμηλότερα
        Size = new Size(BUTTON_WIDTH, BUTTON_HEIGHT)
    };
    btnSave.Click += BtnSave_Click;

    var btnCancel = new Button
    {
        Text = "Cancel",
        BackColor = Color.White,
        Location = new Point(CANCEL_BUTTON_LEFT, 360),
        Size = new Size(BUTTON_WIDTH, BUTTON_HEIGHT)
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
