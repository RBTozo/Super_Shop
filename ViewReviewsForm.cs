using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SuperShop
{
    public class ViewReviewsForm : Form
    {
        private DataGridView dgvReviews;
        private int productId;

        public ViewReviewsForm(int productId, string productName)
        {
            this.productId = productId;
            InitializeComponent(productName);
            LoadReviews();
        }

        private void InitializeComponent(string productName)
        {
            Text = $"Reviews for {productName}";
            Size = new Size(600, 400);
            StartPosition = FormStartPosition.CenterParent;

            dgvReviews = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            Controls.Add(dgvReviews);
        }

        private void LoadReviews()
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string query = @"SELECT U.FullName AS Reviewer, R.Rating, R.Comment, R.ReviewDate 
                             FROM Reviews R 
                             INNER JOIN Users U ON R.UserId = U.UserId 
                             WHERE R.ProductId = @pid 
                             ORDER BY R.ReviewDate DESC";
            
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@pid", productId);

            using var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            dgvReviews.DataSource = dt;
        }
    }
}
