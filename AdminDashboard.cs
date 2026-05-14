using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SuperShop
{
    public class AdminDashboard : Form
    {
        private DataGridView dgvProducts;
        private TextBox txtName, txtCategory, txtBrand, txtModel, txtPrice, txtStock, txtMinStock, txtDesc, txtSearch;
        private Button btnAdd, btnUpdate, btnDelete, btnClear, btnLogout, btnManageOffers;
        private int selectedProductId = 0;

        public AdminDashboard()
        {
            InitializeComponent();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text = "Admin Dashboard - Super Shop";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 244, 248);

            // Header
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(33, 37, 41) };
            Label lblWelcome = new Label { Text = $"Welcome, {Session.FullName} (Admin)", ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(20, 15) };
            btnLogout = new Button { Text = "Logout", Location = new Point(980, 15), Size = new Size(80, 30), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLogout.Click += (s, e) => { Session.Logout(); this.Close(); };
            pnlHeader.Controls.Add(lblWelcome);
            pnlHeader.Controls.Add(btnLogout);

            // Left Panel (Product Form)
            Panel pnlForm = new Panel { Location = new Point(20, 80), Size = new Size(320, 560), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            Label lblFormTitle = new Label { Text = "Manage Items", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };
            
            int y = 40;
            txtName = CreateInput(pnlForm, "Product Name", ref y);
            txtCategory = CreateInput(pnlForm, "Category", ref y);
            txtBrand = CreateInput(pnlForm, "Brand", ref y);
            txtModel = CreateInput(pnlForm, "Model", ref y);
            txtPrice = CreateInput(pnlForm, "Price", ref y);
            txtStock = CreateInput(pnlForm, "Stock Qty", ref y);
            txtMinStock = CreateInput(pnlForm, "Min Stock", ref y);
            txtDesc = CreateInput(pnlForm, "Description", ref y);

            y += 10;
            btnAdd = new Button { Text = "Add", Location = new Point(10, y), Size = new Size(80, 30), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpdate = new Button { Text = "Update", Location = new Point(100, y), Size = new Size(80, 30), BackColor = Color.FromArgb(0, 123, 255), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete = new Button { Text = "Delete", Location = new Point(190, y), Size = new Size(80, 30), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnClear = new Button { Text = "Clear", Location = new Point(10, y + 40), Size = new Size(260, 30), BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += BtnClear_Click;

            pnlForm.Controls.Add(lblFormTitle);
            pnlForm.Controls.Add(btnAdd);
            pnlForm.Controls.Add(btnUpdate);
            pnlForm.Controls.Add(btnDelete);
            pnlForm.Controls.Add(btnClear);

            // Right Panel (Grid)
            Panel pnlGrid = new Panel { Location = new Point(360, 80), Size = new Size(700, 560), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            txtSearch = new TextBox { Location = new Point(10, 10), Size = new Size(300, 25) };
            Button btnSearch = new Button { Text = "Search", Location = new Point(320, 9), Size = new Size(80, 27) };
            btnSearch.Click += (s, e) => LoadProducts(txtSearch.Text);

            Button btnManageOrders = new Button { Text = "Manage Orders", Location = new Point(410, 9), Size = new Size(120, 27), BackColor = Color.FromArgb(23, 162, 184), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnManageOrders.Click += (s, e) => { var mf = new ManageBillsForm(); mf.ShowDialog(); };

            btnManageOffers = new Button { Text = "Manage Offers", Location = new Point(540, 9), Size = new Size(120, 27), BackColor = Color.FromArgb(255, 159, 28), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnManageOffers.Click += (s, e) => { var of = new ManageOffersForm(); of.ShowDialog(); };

            dgvProducts = new DataGridView 
            { 
                Location = new Point(10, 50), 
                Size = new Size(670, 490), 
                ReadOnly = true, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };
            dgvProducts.CellClick += DgvProducts_CellClick;
            dgvProducts.CellFormatting += DgvProducts_CellFormatting;

            pnlGrid.Controls.Add(txtSearch);
            pnlGrid.Controls.Add(btnSearch);
            pnlGrid.Controls.Add(btnManageOrders);
            pnlGrid.Controls.Add(btnManageOffers);
            pnlGrid.Controls.Add(dgvProducts);

            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlForm);
            this.Controls.Add(pnlGrid);
        }

        private TextBox CreateInput(Panel parent, string label, ref int y)
        {
            Label lbl = new Label { Text = label + ":", Location = new Point(10, y), Size = new Size(280, 15) };
            TextBox txt = new TextBox { Location = new Point(10, y + 20), Size = new Size(280, 25) };
            parent.Controls.Add(lbl);
            parent.Controls.Add(txt);
            y += 45;
            return txt;
        }

        private void LoadProducts(string search = "")
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM Products";
                if (!string.IsNullOrEmpty(search)) query += " WHERE ProductName LIKE @s OR Category LIKE @s OR Brand LIKE @s";
                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(search)) cmd.Parameters.AddWithValue("@s", "%" + search + "%");
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvProducts.DataSource = dt;
                    }
                }
            }
        }

        private void DgvProducts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvProducts.Columns[e.ColumnIndex].Name == "StockQuantity")
            {
                int stock = Convert.ToInt32(e.Value);
                int minStock = Convert.ToInt32(dgvProducts.Rows[e.RowIndex].Cells["MinimumStock"].Value);
                if (stock < minStock)
                {
                    e.CellStyle.BackColor = Color.LightCoral;
                    e.CellStyle.ForeColor = Color.White;
                }
            }
        }

        private void DgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvProducts.Rows[e.RowIndex];
                selectedProductId = Convert.ToInt32(row.Cells["ProductId"].Value);
                txtName.Text = row.Cells["ProductName"].Value.ToString();
                txtCategory.Text = row.Cells["Category"].Value.ToString();
                txtBrand.Text = row.Cells["Brand"].Value.ToString();
                txtModel.Text = row.Cells["Model"].Value.ToString();
                txtPrice.Text = row.Cells["Price"].Value.ToString();
                txtStock.Text = row.Cells["StockQuantity"].Value.ToString();
                txtMinStock.Text = row.Cells["MinimumStock"].Value.ToString();
                txtDesc.Text = row.Cells["Description"].Value.ToString();
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO Products (ProductName, Category, Brand, Model, Price, StockQuantity, MinimumStock, Description) VALUES (@n, @c, @b, @m, @p, @s, @ms, @d)";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@n", txtName.Text);
                        cmd.Parameters.AddWithValue("@c", txtCategory.Text);
                        cmd.Parameters.AddWithValue("@b", txtBrand.Text);
                        cmd.Parameters.AddWithValue("@m", txtModel.Text);
                        cmd.Parameters.AddWithValue("@p", Convert.ToDecimal(txtPrice.Text));
                        cmd.Parameters.AddWithValue("@s", Convert.ToInt32(txtStock.Text));
                        cmd.Parameters.AddWithValue("@ms", Convert.ToInt32(txtMinStock.Text));
                        cmd.Parameters.AddWithValue("@d", txtDesc.Text);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Product Added!");
                LoadProducts();
                BtnClear_Click(null, null);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedProductId == 0) return;
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "UPDATE Products SET ProductName=@n, Category=@c, Brand=@b, Model=@m, Price=@p, StockQuantity=@s, MinimumStock=@ms, Description=@d WHERE ProductId=@id";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@n", txtName.Text);
                        cmd.Parameters.AddWithValue("@c", txtCategory.Text);
                        cmd.Parameters.AddWithValue("@b", txtBrand.Text);
                        cmd.Parameters.AddWithValue("@m", txtModel.Text);
                        cmd.Parameters.AddWithValue("@p", Convert.ToDecimal(txtPrice.Text));
                        cmd.Parameters.AddWithValue("@s", Convert.ToInt32(txtStock.Text));
                        cmd.Parameters.AddWithValue("@ms", Convert.ToInt32(txtMinStock.Text));
                        cmd.Parameters.AddWithValue("@d", txtDesc.Text);
                        cmd.Parameters.AddWithValue("@id", selectedProductId);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Product Updated!");
                LoadProducts();
                BtnClear_Click(null, null);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedProductId == 0) return;
            if (MessageBox.Show("Are you sure?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM Products WHERE ProductId=@id";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", selectedProductId);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Product Deleted!");
                LoadProducts();
                BtnClear_Click(null, null);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            selectedProductId = 0;
            txtName.Clear(); txtCategory.Clear(); txtBrand.Clear(); txtModel.Clear(); 
            txtPrice.Clear(); txtStock.Clear(); txtMinStock.Clear(); txtDesc.Clear();
        }
    }
}
