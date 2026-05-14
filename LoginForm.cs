using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace SuperShop
{
    public class LoginForm : Form
    {
        private TextBox txtEmail;
        private TextBox txtPassword;
        private Button btnLogin;
        private LinkLabel lnkRegister;
        private Label lblTitle;
        private Label lblEmail;
        private Label lblPassword;
        private Panel panelMain;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.txtEmail = new TextBox();
            this.txtPassword = new TextBox();
            this.btnLogin = new Button();
            this.lnkRegister = new LinkLabel();
            this.lblTitle = new Label();
            this.lblEmail = new Label();
            this.lblPassword = new Label();
            this.panelMain = new Panel();

            this.SuspendLayout();

            // Form
            this.Text = "Login - Super Shop";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 244, 248);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Panel
            panelMain.BackColor = Color.White;
            panelMain.Size = new Size(320, 380);
            panelMain.Location = new Point(30, 30);
            panelMain.Padding = new Padding(20);
            panelMain.BorderStyle = BorderStyle.FixedSingle;

            // Title
            lblTitle.Text = "Super Shop\nLogin";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(33, 37, 41);
            lblTitle.Size = new Size(280, 60);
            lblTitle.Location = new Point(20, 20);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // Email Label
            lblEmail.Text = "Email:";
            lblEmail.Font = new Font("Segoe UI", 10);
            lblEmail.Location = new Point(20, 100);
            lblEmail.Size = new Size(280, 20);

            // Email TextBox
            txtEmail.Font = new Font("Segoe UI", 12);
            txtEmail.Location = new Point(20, 125);
            txtEmail.Size = new Size(280, 30);

            // Password Label
            lblPassword.Text = "Password:";
            lblPassword.Font = new Font("Segoe UI", 10);
            lblPassword.Location = new Point(20, 170);
            lblPassword.Size = new Size(280, 20);

            // Password TextBox
            txtPassword.Font = new Font("Segoe UI", 12);
            txtPassword.Location = new Point(20, 195);
            txtPassword.Size = new Size(280, 30);
            txtPassword.PasswordChar = '*';

            // Login Button
            btnLogin.Text = "LOGIN";
            btnLogin.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnLogin.BackColor = Color.FromArgb(0, 123, 255);
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Size = new Size(280, 40);
            btnLogin.Location = new Point(20, 250);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Click += BtnLogin_Click;

            // Register Link
            lnkRegister.Text = "Don't have an account? Register here";
            lnkRegister.Font = new Font("Segoe UI", 9);
            lnkRegister.Location = new Point(20, 310);
            lnkRegister.Size = new Size(280, 20);
            lnkRegister.TextAlign = ContentAlignment.MiddleCenter;
            lnkRegister.LinkClicked += LnkRegister_LinkClicked;

            panelMain.Controls.Add(lblTitle);
            panelMain.Controls.Add(lblEmail);
            panelMain.Controls.Add(txtEmail);
            panelMain.Controls.Add(lblPassword);
            panelMain.Controls.Add(txtPassword);
            panelMain.Controls.Add(btnLogin);
            panelMain.Controls.Add(lnkRegister);

            this.Controls.Add(panelMain);
            this.ResumeLayout(false);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both email and password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT UserId, FullName, Role FROM Users WHERE Email = @Email AND Password = @Password";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Password", password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Session.UserId = Convert.ToInt32(reader["UserId"]);
                                Session.FullName = reader["FullName"].ToString();
                                Session.Role = reader["Role"].ToString();

                                this.Hide();
                                
                                Form dashboard = null;
                                switch (Session.Role)
                                {
                                    case "Super Admin":
                                        dashboard = new SuperAdminDashboard();
                                        break;
                                    case "Admin":
                                        dashboard = new AdminDashboard();
                                        break;
                                    case "Salesman":
                                        dashboard = new SalesmanDashboard();
                                        break;
                                    case "Customer":
                                        dashboard = new CustomerDashboard();
                                        break;
                                    default:
                                        MessageBox.Show("Invalid Role Assigned.");
                                        this.Show();
                                        return;
                                }

                                dashboard.FormClosed += (s, args) => this.Show();
                                dashboard.Show();
                                
                                // Clear fields
                                txtEmail.Clear();
                                txtPassword.Clear();
                            }
                            else
                            {
                                MessageBox.Show("Invalid email or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LnkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var regForm = new RegistrationForm();
            regForm.ShowDialog();
        }
    }
}
