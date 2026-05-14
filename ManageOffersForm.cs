using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SuperShop
{
    public class ManageOffersForm : Form
    {
        private DataGridView dgvOffers;
        private ComboBox cmbProducts;
        private NumericUpDown nudDiscount;
        private DateTimePicker dtStart, dtEnd;
        private Button btnSave, btnDelete, btnRefresh;
        private int selectedOfferId = 0;

        public ManageOffersForm()
        {
            InitializeComponent();
            LoadProducts();
            LoadOffers();
        }

        private void InitializeComponent()
        {
            Text = "Manage Offers";
            Size = new Size(980, 600);
            StartPosition = FormStartPosition.CenterParent;

            dgvOffers = new DataGridView
            {
                Location = new Point(10, 10),
                Size = new Size(940, 300),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false
            };
            dgvOffers.CellClick += DgvOffers_CellClick;

            Label lblProduct = new Label { Text = "Product", Location = new Point(10, 330), AutoSize = true };
            cmbProducts = new ComboBox { Location = new Point(10, 350), Size = new Size(260, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblDiscount = new Label { Text = "Discount %", Location = new Point(290, 330), AutoSize = true };
            nudDiscount = new NumericUpDown { Location = new Point(290, 350), Size = new Size(100, 25), Minimum = 0, Maximum = 100, DecimalPlaces = 2 };

            Label lblStart = new Label { Text = "Start Date", Location = new Point(410, 330), AutoSize = true };
            dtStart = new DateTimePicker { Location = new Point(410, 350), Size = new Size(200, 25) };

            Label lblEnd = new Label { Text = "End Date", Location = new Point(630, 330), AutoSize = true };
            dtEnd = new DateTimePicker { Location = new Point(630, 350), Size = new Size(200, 25) };

            btnSave = new Button { Text = "Save Offer", Location = new Point(10, 400), Size = new Size(100, 30), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete = new Button { Text = "Delete Offer", Location = new Point(120, 400), Size = new Size(110, 30), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRefresh = new Button { Text = "Refresh", Location = new Point(240, 400), Size = new Size(80, 30) };

            btnSave.Click += BtnSave_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => LoadOffers();

            Controls.Add(dgvOffers);
            Controls.Add(lblProduct);
            Controls.Add(cmbProducts);
            Controls.Add(lblDiscount);
            Controls.Add(nudDiscount);
            Controls.Add(lblStart);
            Controls.Add(dtStart);
            Controls.Add(lblEnd);
            Controls.Add(dtEnd);
            Controls.Add(btnSave);
            Controls.Add(btnDelete);
            Controls.Add(btnRefresh);
        }

        private void LoadProducts()
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT ProductId, ProductName FROM Products ORDER BY ProductName", conn);
            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            cmbProducts.DisplayMember = "ProductName";
            cmbProducts.ValueMember = "ProductId";
            cmbProducts.DataSource = dt;
        }

        private void LoadOffers()
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var da = new SqlDataAdapter(@"SELECT O.OfferId, O.ProductId, P.ProductName, O.DiscountPercent, O.StartDate, O.EndDate
                                                FROM Offers O
                                                INNER JOIN Products P ON P.ProductId = O.ProductId
                                                ORDER BY O.EndDate DESC", conn);
            var dt = new DataTable();
            da.Fill(dt);
            dgvOffers.DataSource = dt;
            selectedOfferId = 0;
        }

        private void DgvOffers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvOffers.Rows[e.RowIndex];
            selectedOfferId = Convert.ToInt32(row.Cells["OfferId"].Value);
            cmbProducts.SelectedValue = Convert.ToInt32(row.Cells["ProductId"].Value);
            nudDiscount.Value = Convert.ToDecimal(row.Cells["DiscountPercent"].Value);
            dtStart.Value = Convert.ToDateTime(row.Cells["StartDate"].Value);
            dtEnd.Value = Convert.ToDateTime(row.Cells["EndDate"].Value);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbProducts.SelectedValue == null) { MessageBox.Show("Select a product."); return; }

            int productId = Convert.ToInt32(cmbProducts.SelectedValue);
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                if (selectedOfferId == 0)
                {
                    using var cmd = new SqlCommand("INSERT INTO Offers (ProductId, DiscountPercent, StartDate, EndDate) VALUES (@pid, @d, @s, @e)", conn);
                    cmd.Parameters.AddWithValue("@pid", productId);
                    cmd.Parameters.AddWithValue("@d", nudDiscount.Value);
                    cmd.Parameters.AddWithValue("@s", dtStart.Value);
                    cmd.Parameters.AddWithValue("@e", dtEnd.Value);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    using var cmd = new SqlCommand("UPDATE Offers SET ProductId=@pid, DiscountPercent=@d, StartDate=@s, EndDate=@e WHERE OfferId=@id", conn);
                    cmd.Parameters.AddWithValue("@pid", productId);
                    cmd.Parameters.AddWithValue("@d", nudDiscount.Value);
                    cmd.Parameters.AddWithValue("@s", dtStart.Value);
                    cmd.Parameters.AddWithValue("@e", dtEnd.Value);
                    cmd.Parameters.AddWithValue("@id", selectedOfferId);
                    cmd.ExecuteNonQuery();
                }

                LoadOffers();
                MessageBox.Show("Offer saved.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving offer: " + ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedOfferId == 0) { MessageBox.Show("Select an offer to delete."); return; }

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SqlCommand("DELETE FROM Offers WHERE OfferId=@id", conn);
                cmd.Parameters.AddWithValue("@id", selectedOfferId);
                cmd.ExecuteNonQuery();
                selectedOfferId = 0;
                LoadOffers();
                MessageBox.Show("Offer deleted.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting offer: " + ex.Message);
            }
        }
    }
}
