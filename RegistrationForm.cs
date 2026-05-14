using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace SuperShop
{
    public class RegistrationForm : Form
    {
        private TextBox txtFullName, txtEmail, txtPhone, txtAddress, txtPassword, txtConfirmPassword;
        private Button btnRegister;
        private Label lblTitle;
        private Panel panelMain;

        public RegistrationForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Register - Super Shop";
            this.Size = new Size(450, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 244, 248);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            panelMain = new Panel
            {
                BackColor = Color.White,
                Size = new Size(390, 550),
                Location = new Point(20, 20),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblTitle = new Label
            {
                Text = "Customer Registration",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Size = new Size(350, 40),
                Location = new Point(20, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            int startY = 80;
            int yStep = 60;

            txtFullName = CreateInput(panelMain, "Full Name:", startY, out _);
            txtEmail = CreateInput(panelMain, "Email:", startY += yStep, out _);
            txtPhone = CreateInput(panelMain, "Phone:", startY += yStep, out _);
            txtAddress = CreateInput(panelMain, "Address:", startY += yStep, out _);
            txtPassword = CreateInput(panelMain, "Password:", startY += yStep, out _);
            txtPassword.PasswordChar = '*';
            txtConfirmPassword = CreateInput(panelMain, "Confirm Password:", startY += yStep, out _);
            txtConfirmPassword.PasswordChar = '*';

            btnRegister = new Button
            {
                Text = "REGISTER",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(350, 40),
                Location = new Point(20, startY + 60),
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;

            panelMain.Controls.Add(lblTitle);
            panelMain.Controls.Add(btnRegister);
            this.Controls.Add(panelMain);
        }

        private TextBox CreateInput(Panel parent, string labelText, int y, out Label lbl)
        {
            lbl = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, y),
                Size = new Size(350, 15)
            };
            TextBox txt = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(20, y + 20),
                Size = new Size(350, 30)
            };
            parent.Controls.Add(lbl);
            parent.Controls.Add(txt);
            return txt;
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text) || string.IsNullOrWhiteSpace(txtConfirmPassword.Text))
            {
                MessageBox.Show("Please fill all required fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    
                    // Check if email exists
                    using (var checkCmd = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Email = @Email", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        long count = (long)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Email is already registered.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // Insert new customer
                    string query = "INSERT INTO Users (FullName, Email, Phone, Address, Password, Role) VALUES (@FullName, @Email, @Phone, @Address, @Password, 'Customer')";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                        cmd.Parameters.AddWithValue("@Password", txtPassword.Text);
                        
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Registration successful. You can now login.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
