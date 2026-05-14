using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SuperShop
{
    public class SuperAdminDashboard : Form
    {
        private DataGridView dgvUsers;
        private TextBox txtFullName, txtEmail, txtPhone, txtAddress, txtPassword, txtSearch;
        private ComboBox cbRole;
        private Button btnAdd, btnUpdate, btnDelete, btnClear, btnLogout;
        private Label lblWelcome;
        private int selectedUserId = 0;

        public SuperAdminDashboard()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.Text = "Super Admin Dashboard - Super Shop";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 244, 248);

            // Header Panel
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(33, 37, 41) };
            
            lblWelcome = new Label { Text = $"Welcome, {Session.FullName} (Super Admin)", ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(20, 15) };
            btnLogout = new Button { Text = "Logout", Location = new Point(880, 15), Size = new Size(80, 30), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => { Session.Logout(); this.Close(); };
            
            pnlHeader.Controls.Add(lblWelcome);
            pnlHeader.Controls.Add(btnLogout);

            // Left Panel (Form)
            Panel pnlForm = new Panel { Location = new Point(20, 80), Size = new Size(300, 500), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            
            Label lblFormTitle = new Label { Text = "Manage Users", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };
            
            int y = 40;
            txtFullName = CreateLabeledTextBox(pnlForm, "Full Name", ref y);
            txtEmail = CreateLabeledTextBox(pnlForm, "Email", ref y);
            txtPhone = CreateLabeledTextBox(pnlForm, "Phone", ref y);
            txtAddress = CreateLabeledTextBox(pnlForm, "Address", ref y);
            txtPassword = CreateLabeledTextBox(pnlForm, "Password", ref y);
            
            Label lblRole = new Label { Text = "Role:", Location = new Point(10, y), Size = new Size(280, 15) };
            cbRole = new ComboBox { Location = new Point(10, y + 20), Size = new Size(270, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cbRole.Items.AddRange(new string[] { "Admin", "Salesman", "Customer" });
            cbRole.SelectedIndex = 0;
            y += 50;

            btnAdd = new Button { Text = "Add", Location = new Point(10, y), Size = new Size(80, 30), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpdate = new Button { Text = "Update", Location = new Point(100, y), Size = new Size(80, 30), BackColor = Color.FromArgb(0, 123, 255), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete = new Button { Text = "Delete", Location = new Point(190, y), Size = new Size(80, 30), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnClear = new Button { Text = "Clear", Location = new Point(10, y + 40), Size = new Size(260, 30), BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += BtnClear_Click;

            pnlForm.Controls.Add(lblFormTitle);
            pnlForm.Controls.Add(lblRole);
            pnlForm.Controls.Add(cbRole);
            pnlForm.Controls.Add(btnAdd);
            pnlForm.Controls.Add(btnUpdate);
            pnlForm.Controls.Add(btnDelete);
            pnlForm.Controls.Add(btnClear);

            // Right Panel (Grid)
            Panel pnlGrid = new Panel { Location = new Point(340, 80), Size = new Size(620, 500), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            
            txtSearch = new TextBox { Location = new Point(10, 10), Size = new Size(250, 25) };
            Button btnSearch = new Button { Text = "Search", Location = new Point(270, 9), Size = new Size(80, 27) };
            btnSearch.Click += (s, e) => LoadUsers(txtSearch.Text);

            dgvUsers = new DataGridView 
            { 
                Location = new Point(10, 50), 
                Size = new Size(600, 440), 
                ReadOnly = true, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };
            dgvUsers.CellClick += DgvUsers_CellClick;

            pnlGrid.Controls.Add(txtSearch);
            pnlGrid.Controls.Add(btnSearch);
            pnlGrid.Controls.Add(dgvUsers);

            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlForm);
            this.Controls.Add(pnlGrid);
        }

        private TextBox CreateLabeledTextBox(Panel parent, string label, ref int y)
        {
            Label lbl = new Label { Text = label + ":", Location = new Point(10, y), Size = new Size(280, 15) };
            TextBox txt = new TextBox { Location = new Point(10, y + 20), Size = new Size(270, 25) };
            parent.Controls.Add(lbl);
            parent.Controls.Add(txt);
            y += 50;
            return txt;
        }

        private void LoadUsers(string searchTerm = "")
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = "SELECT UserId, FullName, Email, Phone, Address, Role, JoiningDate FROM Users WHERE Role != 'Super Admin'";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += " AND (FullName LIKE @search OR Email LIKE @search OR Role LIKE @search)";
                }
                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(searchTerm)) cmd.Parameters.AddWithValue("@search", "%" + searchTerm + "%");
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvUsers.DataSource = dt;
                    }
                }
            }
        }

        private void DgvUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvUsers.Rows[e.RowIndex];
                selectedUserId = Convert.ToInt32(row.Cells["UserId"].Value);
                txtFullName.Text = row.Cells["FullName"].Value.ToString();
                txtEmail.Text = row.Cells["Email"].Value.ToString();
                txtPhone.Text = row.Cells["Phone"].Value.ToString();
                txtAddress.Text = row.Cells["Address"].Value.ToString();
                cbRole.Text = row.Cells["Role"].Value.ToString();
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = "INSERT INTO Users (FullName, Email, Phone, Address, Password, Role) VALUES (@fn, @em, @ph, @ad, @pw, @rl)";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@fn", txtFullName.Text);
                    cmd.Parameters.AddWithValue("@em", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@ph", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@ad", txtAddress.Text);
                    cmd.Parameters.AddWithValue("@pw", string.IsNullOrEmpty(txtPassword.Text) ? "1234" : txtPassword.Text);
                    cmd.Parameters.AddWithValue("@rl", cbRole.Text);
                    cmd.ExecuteNonQuery();
                }
            }
            MessageBox.Show("User Added!");
            LoadUsers();
            BtnClear_Click(null, null);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedUserId == 0) return;
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = "UPDATE Users SET FullName=@fn, Email=@em, Phone=@ph, Address=@ad, Role=@rl WHERE UserId=@id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@fn", txtFullName.Text);
                    cmd.Parameters.AddWithValue("@em", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@ph", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@ad", txtAddress.Text);
                    cmd.Parameters.AddWithValue("@rl", cbRole.Text);
                    cmd.Parameters.AddWithValue("@id", selectedUserId);
                    cmd.ExecuteNonQuery();
                }
            }
            MessageBox.Show("User Updated!");
            LoadUsers();
            BtnClear_Click(null, null);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedUserId == 0) return;
            if (MessageBox.Show("Are you sure?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM Users WHERE UserId=@id";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", selectedUserId);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("User Deleted!");
                LoadUsers();
                BtnClear_Click(null, null);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            selectedUserId = 0;
            txtFullName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            txtAddress.Clear();
            txtPassword.Clear();
            cbRole.SelectedIndex = 0;
        }
    }
}
