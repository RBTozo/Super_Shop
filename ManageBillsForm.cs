using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SuperShop
{
    public class ManageBillsForm : Form
    {
        private DataGridView dgvBills;
        private ComboBox cmbStatus;
        private Button btnUpdate, btnRefresh;

        public ManageBillsForm()
        {
            InitializeComponent();
            LoadBills();
        }

        private void InitializeComponent()
        {
            this.Text = "Manage Orders";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            dgvBills = new DataGridView { Location = new Point(10, 10), Size = new Size(860, 460), ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false };
            cmbStatus = new ComboBox { Location = new Point(10, 480), Size = new Size(200, 25) };
            cmbStatus.Items.AddRange(new object[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" });
            cmbStatus.SelectedIndex = 0;
            btnUpdate = new Button { Text = "Update Status", Location = new Point(220, 478), Size = new Size(120, 28) };
            btnRefresh = new Button { Text = "Refresh", Location = new Point(350, 478), Size = new Size(80, 28) };

            btnUpdate.Click += BtnUpdate_Click;
            btnRefresh.Click += (s, e) => LoadBills();

            this.Controls.Add(dgvBills);
            this.Controls.Add(cmbStatus);
            this.Controls.Add(btnUpdate);
            this.Controls.Add(btnRefresh);
        }

        private void LoadBills()
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string q = "SELECT BillId, CustomerName, CustomerPhone, TotalAmount, DiscountAmount, FinalAmount, DeliveryStatus, BillDate, SalesmanId FROM Bills ORDER BY BillDate DESC";
            using var da = new SqlDataAdapter(q, conn);
            var dt = new DataTable();
            da.Fill(dt);
            dgvBills.DataSource = dt;
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvBills.SelectedRows.Count == 0) { MessageBox.Show("Select a bill to update."); return; }
            int billId = Convert.ToInt32(dgvBills.SelectedRows[0].Cells["BillId"].Value);
            string status = cmbStatus.SelectedItem.ToString();
            try
            {
                DatabaseHelper.UpdateBillDeliveryStatus(billId, status);
                MessageBox.Show("Status updated.");
                LoadBills();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating status: " + ex.Message);
            }
        }
    }
}
