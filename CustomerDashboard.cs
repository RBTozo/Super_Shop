using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SuperShop
{
    public class CustomerDashboard : Form
    {
        private DataGridView dgvProducts;
        private TextBox txtSearch, txtQuantity;
        private Button btnAddToCart, btnCheckout, btnLogout;
        private Label lblTotal, lblWelcome, lblSelectedInfo;
        
        private DataTable cartTable;
        private int selectedProductId = 0;
        private decimal currentPrice = 0;
        private string currentProductName = "";
        
        // Use a ListBox for a simple cart display
        private ListBox lstCart;

        public CustomerDashboard()
        {
            InitializeComponent();
            InitializeCart();
            LoadProducts();
        }

        private void InitializeCart()
        {
            cartTable = new DataTable();
            cartTable.Columns.Add("ProductId", typeof(int));
            cartTable.Columns.Add("ProductName", typeof(string));
            cartTable.Columns.Add("Price", typeof(decimal));
            cartTable.Columns.Add("Quantity", typeof(int));
            cartTable.Columns.Add("Total", typeof(decimal));
        }

        private void InitializeComponent()
        {
            this.Text = "Customer Dashboard - Super Shop";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 244, 248);

            // Header
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(33, 37, 41) };
            lblWelcome = new Label { Text = $"Welcome, {Session.FullName} (Customer)", ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(20, 15) };
            btnLogout = new Button { Text = "Logout", Location = new Point(980, 15), Size = new Size(80, 30), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLogout.Click += (s, e) => { Session.Logout(); this.Close(); };
            pnlHeader.Controls.Add(lblWelcome);
            pnlHeader.Controls.Add(btnLogout);

            // Left Panel (Products)
            Panel pnlProducts = new Panel { Location = new Point(20, 80), Size = new Size(650, 560), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            Label lblTitle = new Label { Text = "Browse Items", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };
            
            txtSearch = new TextBox { Location = new Point(10, 40), Size = new Size(300, 25) };
            Button btnSearch = new Button { Text = "Search", Location = new Point(320, 39), Size = new Size(80, 27) };
            btnSearch.Click += (s, e) => LoadProducts(txtSearch.Text);

            dgvProducts = new DataGridView { Location = new Point(10, 80), Size = new Size(620, 380), ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, BackgroundColor = Color.White };
            dgvProducts.CellClick += DgvProducts_CellClick;

            Button btnOffers = new Button { Text = "View Offers", Location = new Point(10, 500), Size = new Size(100, 27), BackColor = Color.FromArgb(23, 162, 184), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnOffers.Click += (s, e) => { new ActiveOffersForm().ShowDialog(); };

            Button btnReview = new Button { Text = "Review Selected", Location = new Point(120, 500), Size = new Size(120, 27), BackColor = Color.FromArgb(255, 193, 7), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnReview.Click += (s, e) => {
                if (selectedProductId == 0) { MessageBox.Show("Select a product to review."); return; }
                var rf = new ReviewForm(selectedProductId);
                rf.ShowDialog();
                LoadProducts();
            };

            Button btnViewReviews = new Button { Text = "View Reviews", Location = new Point(250, 500), Size = new Size(120, 27), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnViewReviews.Click += (s, e) => {
                if (selectedProductId == 0) { MessageBox.Show("Select a product to view reviews."); return; }
                new ViewReviewsForm(selectedProductId, currentProductName).ShowDialog();
            };

            Button btnMyOrders = new Button { Text = "My Orders", Location = new Point(380, 500), Size = new Size(100, 27), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnMyOrders.Click += (s, e) => { new ViewOrdersForm("Customer").ShowDialog(); };

            pnlProducts.Controls.Add(lblTitle);
            pnlProducts.Controls.Add(txtSearch);
            pnlProducts.Controls.Add(btnSearch);
            pnlProducts.Controls.Add(dgvProducts);
            pnlProducts.Controls.Add(btnOffers);
            pnlProducts.Controls.Add(btnReview);
            pnlProducts.Controls.Add(btnViewReviews);
            pnlProducts.Controls.Add(btnMyOrders);
            lblSelectedInfo = new Label { Text = "Select a product to see discount and rating details.", Location = new Point(10, 465), Size = new Size(610, 30), ForeColor = Color.DimGray };
            pnlProducts.Controls.Add(lblSelectedInfo);

            // Right Panel (Cart)
            Panel pnlCart = new Panel { Location = new Point(690, 80), Size = new Size(370, 560), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            Label lblCartTitle = new Label { Text = "My Cart", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };

            Label lblQty = new Label { Text = "Qty:", Location = new Point(10, 45), Size = new Size(40, 20) };
            txtQuantity = new TextBox { Location = new Point(50, 42), Size = new Size(50, 25), Text = "1" };
            btnAddToCart = new Button { Text = "Add Selected to Cart", Location = new Point(110, 40), Size = new Size(150, 27), BackColor = Color.FromArgb(0, 123, 255), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddToCart.Click += BtnAddToCart_Click;

            lstCart = new ListBox { Location = new Point(10, 80), Size = new Size(340, 320), Font = new Font("Consolas", 10) };

            Button btnRemove = new Button { Text = "Clear Cart", Location = new Point(10, 410), Size = new Size(100, 30), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRemove.Click += (s, e) => { cartTable.Clear(); UpdateCartView(); };

            lblTotal = new Label { Text = "Total: $0.00", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(40, 167, 69), Location = new Point(120, 410), Size = new Size(230, 30), TextAlign = ContentAlignment.MiddleRight };

            btnCheckout = new Button { Text = "Place Order", Location = new Point(10, 480), Size = new Size(340, 40), Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnCheckout.Click += BtnCheckout_Click;

            pnlCart.Controls.Add(lblCartTitle);
            pnlCart.Controls.Add(lblQty);
            pnlCart.Controls.Add(txtQuantity);
            pnlCart.Controls.Add(btnAddToCart);
            pnlCart.Controls.Add(lstCart);
            pnlCart.Controls.Add(btnRemove);
            pnlCart.Controls.Add(lblTotal);
            pnlCart.Controls.Add(btnCheckout);

            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlProducts);
            this.Controls.Add(pnlCart);
        }

        private void LoadProducts(string search = "")
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                     string query = @"SELECT P.ProductId, P.ProductName, P.Category, P.Brand, P.Price, P.StockQuantity, P.Description,
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
                if (!string.IsNullOrEmpty(search)) query += " AND (ProductName LIKE @s OR Category LIKE @s OR Brand LIKE @s)";
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
            if (selectedProductId == 0) { MessageBox.Show("Please select an item from the list."); return; }
            if (!int.TryParse(txtQuantity.Text, out int qty) || qty <= 0) { MessageBox.Show("Enter a valid quantity."); return; }

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
            
            // Optionally insert into Cart DB table
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string q = "INSERT INTO Cart (UserId, ProductId, Quantity) VALUES (@uid, @pid, @qty)";
                using (var cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", Session.UserId);
                    cmd.Parameters.AddWithValue("@pid", selectedProductId);
                    cmd.Parameters.AddWithValue("@qty", qty);
                    cmd.ExecuteNonQuery();
                }
            }

            UpdateCartView();
        }

        private void UpdateCartView()
        {
            lstCart.Items.Clear();
            decimal total = 0;
            decimal totalDiscount = 0;
            foreach (DataRow row in cartTable.Rows)
            {
                string name = row["ProductName"].ToString();
                if (name.Length > 20) name = name.Substring(0, 17) + "...";
                int pId = (int)row["ProductId"];
                decimal rowTotal = (decimal)row["Total"];
                var pct = DatabaseHelper.GetActiveOfferPercent(pId);
                decimal discountAmt = Math.Round(rowTotal * pct / 100m, 2);
                
                lstCart.Items.Add($"{name,-20} x{row["Quantity"],-2} ${rowTotal:F2}");
                if (discountAmt > 0)
                {
                    lstCart.Items.Add($"  (-${discountAmt:F2} discount)");
                }
                
                total += rowTotal;
                totalDiscount += discountAmt;
            }
            lblTotal.Text = $"Total: ${(total - totalDiscount):F2}";
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0) { MessageBox.Show("Your cart is empty."); return; }

            decimal totalAmount = 0;
            foreach (DataRow row in cartTable.Rows) totalAmount += (decimal)row["Total"];

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    try
                    {
                        string billQuery = "INSERT INTO Bills (CustomerName, CustomerPhone, TotalAmount, DiscountAmount, FinalAmount, DeliveryStatus, SalesmanId) VALUES (@cn, @cp, @ta, @da, @fa, @ds, 0); SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";
                        long billId = 0;
                        decimal totalDiscount = 0m;
                        using (var cmd = new SqlCommand(billQuery, conn))
                        {
                            cmd.Transaction = tr;
                            cmd.Parameters.AddWithValue("@cn", Session.FullName);
                            cmd.Parameters.AddWithValue("@cp", "N/A"); // Retrieve from user table if needed
                            cmd.Parameters.AddWithValue("@ta", totalAmount);
                            // compute discounts
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
                            billId = Convert.ToInt64(cmd.ExecuteScalar());
                        }

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
                        
                        // Clear Cart Table for this user
                        string clearCartQ = "DELETE FROM Cart WHERE UserId = @uid";
                        using (var cmd = new SqlCommand(clearCartQ, conn))
                        {
                            cmd.Transaction = tr;
                            cmd.Parameters.AddWithValue("@uid", Session.UserId);
                            cmd.ExecuteNonQuery();
                        }

                        tr.Commit();
                        decimal finalAmount = totalAmount - totalDiscount;
                        MessageBox.Show($"Order Placed Successfully!\nThank you, {Session.FullName}. Your total was ${finalAmount:F2}.", "Order Confirmed");
                        
                        cartTable.Clear();
                        UpdateCartView();
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
