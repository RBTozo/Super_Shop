using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace SuperShop
{
    public class SalesmanDashboard : Form
    {
        private DataGridView dgvProducts, dgvCart;
        private TextBox txtSearch, txtCustName, txtCustPhone, txtQuantity;
        private Label lblTotal, lblWelcome, lblSelectedInfo;
        private Button btnAddToCart, btnRemove, btnCheckout, btnLogout;
        
        private DataTable cartTable;
        private int selectedProductId = 0;
        private decimal currentPrice = 0;
        private string currentProductName = "";

        public SalesmanDashboard()
        {
            InitializeComponent();
            InitializeCartTable();
            LoadProducts();
        }

        private void InitializeCartTable()
        {
            cartTable = new DataTable();
            cartTable.Columns.Add("ProductId", typeof(int));
            cartTable.Columns.Add("ProductName", typeof(string));
            cartTable.Columns.Add("Price", typeof(decimal));
            cartTable.Columns.Add("Quantity", typeof(int));
            cartTable.Columns.Add("Total", typeof(decimal));
            dgvCart.DataSource = cartTable;
            dgvCart.Columns["ProductId"].Visible = false;
        }

        private void InitializeComponent()
        {
            this.Text = "Salesman Dashboard - Billing - Super Shop";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 244, 248);

            // Header
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(33, 37, 41) };
            lblWelcome = new Label { Text = $"Welcome, {Session.FullName} (Salesman)", ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(20, 15) };
            btnLogout = new Button { Text = "Logout", Location = new Point(1080, 15), Size = new Size(80, 30), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLogout.Click += (s, e) => { Session.Logout(); this.Close(); };
            pnlHeader.Controls.Add(lblWelcome);
            pnlHeader.Controls.Add(btnLogout);

            // Products Panel
            Panel pnlProducts = new Panel { Location = new Point(20, 80), Size = new Size(600, 560), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            Label lblProdTitle = new Label { Text = "Available Items", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };
            
            txtSearch = new TextBox { Location = new Point(10, 40), Size = new Size(300, 25) };
            Button btnSearch = new Button { Text = "Search", Location = new Point(320, 39), Size = new Size(80, 27) };
            btnSearch.Click += (s, e) => LoadProducts(txtSearch.Text);

            dgvProducts = new DataGridView { Location = new Point(10, 80), Size = new Size(570, 380), ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, BackgroundColor = Color.White };
            dgvProducts.CellClick += DgvProducts_CellClick;

            pnlProducts.Controls.Add(lblProdTitle);
            pnlProducts.Controls.Add(txtSearch);
            pnlProducts.Controls.Add(btnSearch);
            pnlProducts.Controls.Add(dgvProducts);
            Button btnOffers = new Button { Text = "View Offers", Location = new Point(430, 39), Size = new Size(120, 27), BackColor = Color.FromArgb(23, 162, 184), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnOffers.Click += (s, e) => { new ActiveOffersForm().ShowDialog(); };
            pnlProducts.Controls.Add(btnOffers);

            Button btnViewReviews = new Button { Text = "View Reviews", Location = new Point(10, 500), Size = new Size(120, 27), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnViewReviews.Click += (s, e) => {
                if (selectedProductId == 0) { MessageBox.Show("Select a product to view reviews."); return; }
                new ViewReviewsForm(selectedProductId, currentProductName).ShowDialog();
            };
            pnlProducts.Controls.Add(btnViewReviews);

            Button btnMyOrders = new Button { Text = "My Orders", Location = new Point(140, 500), Size = new Size(100, 27), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnMyOrders.Click += (s, e) => { new ViewOrdersForm("Salesman").ShowDialog(); };
            pnlProducts.Controls.Add(btnMyOrders);

            lblSelectedInfo = new Label { Text = "Select a product to see discount and rating details.", Location = new Point(10, 465), Size = new Size(560, 30), ForeColor = Color.DimGray };
            pnlProducts.Controls.Add(lblSelectedInfo);

            // Billing / Cart Panel
            Panel pnlBilling = new Panel { Location = new Point(640, 80), Size = new Size(520, 560), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            Label lblBillTitle = new Label { Text = "Current Bill / Cart", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };

            Label lblQty = new Label { Text = "Quantity:", Location = new Point(10, 45), Size = new Size(60, 20) };
            txtQuantity = new TextBox { Location = new Point(70, 42), Size = new Size(80, 25), Text = "1" };
            btnAddToCart = new Button { Text = "Add to Cart", Location = new Point(160, 40), Size = new Size(100, 27), BackColor = Color.FromArgb(0, 123, 255), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddToCart.Click += BtnAddToCart_Click;

            dgvCart = new DataGridView { Location = new Point(10, 80), Size = new Size(490, 280), ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, BackgroundColor = Color.White };

            btnRemove = new Button { Text = "Remove Item", Location = new Point(10, 370), Size = new Size(100, 30), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRemove.Click += BtnRemove_Click;

            lblTotal = new Label { Text = "Total Amount: $0.00", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(40, 167, 69), Location = new Point(250, 370), Size = new Size(250, 30), TextAlign = ContentAlignment.MiddleRight };

            Label lblCustName = new Label { Text = "Customer Name:", Location = new Point(10, 430), Size = new Size(100, 20) };
            txtCustName = new TextBox { Location = new Point(120, 427), Size = new Size(200, 25) };

            Label lblCustPhone = new Label { Text = "Customer Phone:", Location = new Point(10, 470), Size = new Size(100, 20) };
            txtCustPhone = new TextBox { Location = new Point(120, 467), Size = new Size(200, 25) };

            btnCheckout = new Button { Text = "Confirm & Checkout", Location = new Point(10, 510), Size = new Size(490, 40), Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnCheckout.Click += BtnCheckout_Click;

            pnlBilling.Controls.Add(lblBillTitle);
            pnlBilling.Controls.Add(lblQty);
            pnlBilling.Controls.Add(txtQuantity);
            pnlBilling.Controls.Add(btnAddToCart);
            pnlBilling.Controls.Add(dgvCart);
            pnlBilling.Controls.Add(btnRemove);
            pnlBilling.Controls.Add(lblTotal);
            pnlBilling.Controls.Add(lblCustName);
            pnlBilling.Controls.Add(txtCustName);
            pnlBilling.Controls.Add(lblCustPhone);
            pnlBilling.Controls.Add(txtCustPhone);
            pnlBilling.Controls.Add(btnCheckout);

            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlProducts);
            this.Controls.Add(pnlBilling);
        }

        private void LoadProducts(string search = "")
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"SELECT P.ProductId, P.ProductName, P.Brand, P.Price, P.StockQuantity,
                                    ISNULL(O.DiscountPercent, 0) AS ActiveDiscount,
                                    CAST(P.Price - (P.Price * ISNULL(O.DiscountPercent, 0) / 100.0) AS DECIMAL(10,2)) AS DiscountedPrice,
                                    ISNULL((SELECT AVG(CAST(Rating AS FLOAT)) FROM Reviews R WHERE R.ProductId = P.ProductId), 0) AS AvgRating
                                 FROM Products P
                                 OUTER APPLY (
                                    SELECT TOP 1 DiscountPercent
                                    FROM Offers O
                                    WHERE O.ProductId = P.ProductId AND GETDATE() BETWEEN O.StartDate AND O.EndDate
                                    ORDER BY DiscountPercent DESC
                                 ) O
                                 WHERE P.StockQuantity > 0";
                if (!string.IsNullOrEmpty(search)) query += " AND (P.ProductName LIKE @s OR P.Brand LIKE @s)";
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

        private void DgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvProducts.Rows[e.RowIndex];
                selectedProductId = Convert.ToInt32(row.Cells["ProductId"].Value);
                currentProductName = row.Cells["ProductName"].Value.ToString();
                currentPrice = Convert.ToDecimal(row.Cells["Price"].Value);
                decimal discount = Convert.ToDecimal(row.Cells["ActiveDiscount"].Value);
                decimal discountedPrice = Convert.ToDecimal(row.Cells["DiscountedPrice"].Value);
                decimal rating = Convert.ToDecimal(row.Cells["AvgRating"].Value);
                lblSelectedInfo.Text = $"Offer: {discount:F0}% | Discounted price: ${discountedPrice:F2} | Avg rating: {rating:F1}/5";
            }
        }

        private void BtnAddToCart_Click(object sender, EventArgs e)
        {
            if (selectedProductId == 0) { MessageBox.Show("Select a product first."); return; }
            if (!int.TryParse(txtQuantity.Text, out int qty) || qty <= 0) { MessageBox.Show("Enter valid quantity."); return; }

            // Check stock logic could be added here
            
            bool found = false;
            foreach (DataRow row in cartTable.Rows)
            {
                if ((int)row["ProductId"] == selectedProductId)
                {
                    row["Quantity"] = (int)row["Quantity"] + qty;
                    row["Total"] = (int)row["Quantity"] * (decimal)row["Price"];
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                cartTable.Rows.Add(selectedProductId, currentProductName, currentPrice, qty, currentPrice * qty);
            }
            UpdateTotal();
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count > 0)
            {
                dgvCart.Rows.RemoveAt(dgvCart.SelectedRows[0].Index);
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            decimal total = 0;
            decimal totalDiscount = 0;
            foreach (DataRow row in cartTable.Rows)
            {
                decimal rowTotal = (decimal)row["Total"];
                total += rowTotal;
                int pId = (int)row["ProductId"];
                var pct = DatabaseHelper.GetActiveOfferPercent(pId);
                decimal discountAmt = Math.Round(rowTotal * pct / 100m, 2);
                totalDiscount += discountAmt;
            }
            if (totalDiscount > 0)
            {
                lblTotal.Text = $"Total Amount: ${(total - totalDiscount):F2} (-${totalDiscount:F2})";
            }
            else
            {
                lblTotal.Text = $"Total Amount: ${total:F2}";
            }
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0) { MessageBox.Show("Cart is empty."); return; }
            if (string.IsNullOrWhiteSpace(txtCustName.Text)) { MessageBox.Show("Enter customer name."); return; }

            decimal totalAmount = 0;
            foreach (DataRow row in cartTable.Rows) totalAmount += (decimal)row["Total"];

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    try
                    {
                        // Insert Bill (includes discount and delivery status)
                        string billQuery = "INSERT INTO Bills (CustomerName, CustomerPhone, TotalAmount, DiscountAmount, FinalAmount, DeliveryStatus, SalesmanId) VALUES (@cn, @cp, @ta, @da, @fa, @ds, @sid); SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";
                        long billId = 0;
                        decimal totalDiscount = 0m;
                        using (var cmd = new SqlCommand(billQuery, conn))
                        {
                            cmd.Transaction = tr;
                            cmd.Parameters.AddWithValue("@cn", txtCustName.Text);
                            cmd.Parameters.AddWithValue("@cp", txtCustPhone.Text);
                            cmd.Parameters.AddWithValue("@ta", totalAmount);
                            foreach (DataRow row in cartTable.Rows)
                            {
                                int pId = (int)row["ProductId"];
                                decimal itemTotal = (decimal)row["Total"];
                                var pct = DatabaseHelper.GetActiveOfferPercent(pId);
                                if (pct > 0) totalDiscount += Math.Round(itemTotal * pct / 100m, 2);
                            }
                            cmd.Parameters.AddWithValue("@da", totalDiscount);
                            cmd.Parameters.AddWithValue("@fa", totalAmount - totalDiscount);
                            cmd.Parameters.AddWithValue("@ds", "Pending");
                            cmd.Parameters.AddWithValue("@sid", Session.UserId);
                            billId = Convert.ToInt64(cmd.ExecuteScalar());
                        }

                        // Insert Bill Items and Update Stock
                        foreach (DataRow row in cartTable.Rows)
                        {
                            int pId = (int)row["ProductId"];
                            int qty = (int)row["Quantity"];
                            decimal price = (decimal)row["Price"];
                            decimal total = (decimal)row["Total"];

                            string itemQuery = "INSERT INTO BillItems (BillId, ProductId, Quantity, UnitPrice, TotalPrice) VALUES (@bid, @pid, @q, @up, @tp)";
                            using (var cmd = new SqlCommand(itemQuery, conn))
                            {
                                cmd.Transaction = tr;
                                cmd.Parameters.AddWithValue("@bid", billId);
                                cmd.Parameters.AddWithValue("@pid", pId);
                                cmd.Parameters.AddWithValue("@q", qty);
                                cmd.Parameters.AddWithValue("@up", price);
                                cmd.Parameters.AddWithValue("@tp", total);
                                cmd.ExecuteNonQuery();
                            }

                            string stockQuery = "UPDATE Products SET StockQuantity = StockQuantity - @q WHERE ProductId = @pid";
                            using (var cmd = new SqlCommand(stockQuery, conn))
                            {
                                cmd.Transaction = tr;
                                cmd.Parameters.AddWithValue("@q", qty);
                                cmd.Parameters.AddWithValue("@pid", pId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tr.Commit();
                        decimal finalAmount = totalAmount - totalDiscount;
                        MessageBox.Show($"Checkout Successful! Bill ID: {billId}\nTotal: ${finalAmount:F2}", "Success");
                        cartTable.Clear();
                        txtCustName.Clear();
                        txtCustPhone.Clear();
                        UpdateTotal();
                        LoadProducts();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        MessageBox.Show("Error during checkout: " + ex.Message);
                    }
                }
            }
        }
    }
}
