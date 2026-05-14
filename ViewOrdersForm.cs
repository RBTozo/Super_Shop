using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SuperShop
{
    public class ViewOrdersForm : Form
    {
        private DataGridView dgvOrders;
        private string role;

        public ViewOrdersForm(string userRole)
        {
            this.role = userRole;
            InitializeComponent();
            LoadOrders();
        }

        private void InitializeComponent()
        {
            Text = "My Orders - Delivery Status";
            Size = new Size(800, 450);
            StartPosition = FormStartPosition.CenterParent;

            dgvOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            Panel pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            Button btnMarkDelivered = new Button { Text = role == "Customer" ? "Mark as Received" : "Mark as Delivered", Location = new Point(10, 10), Size = new Size(150, 30), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnMarkDelivered.Click += (s, e) => {
                if (dgvOrders.SelectedRows.Count == 0) { MessageBox.Show("Select an order first."); return; }
                int billId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["BillId"].Value);
                DatabaseHelper.UpdateBillDeliveryStatus(billId, "Delivered");
                MessageBox.Show("Status updated successfully!");
                LoadOrders();
            };
            pnlBottom.Controls.Add(btnMarkDelivered);

            Controls.Add(dgvOrders);
            Controls.Add(pnlBottom);
        }

        private void LoadOrders()
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string query = "";
            
            if (role == "Customer")
            {
                query = "SELECT BillId, TotalAmount, DiscountAmount, FinalAmount, DeliveryStatus, BillDate FROM Bills WHERE CustomerName = @name ORDER BY BillDate DESC";
            }
            else if (role == "Salesman")
            {
                query = "SELECT BillId, CustomerName, CustomerPhone, FinalAmount, DeliveryStatus, BillDate FROM Bills WHERE SalesmanId = @id ORDER BY BillDate DESC";
            }

            using var cmd = new SqlCommand(query, conn);
            if (role == "Customer") cmd.Parameters.AddWithValue("@name", Session.FullName);
            else if (role == "Salesman") cmd.Parameters.AddWithValue("@id", Session.UserId);

            using var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            dgvOrders.DataSource = dt;
        }
    }
}
