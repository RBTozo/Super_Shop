using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SuperShop
{
    public class ActiveOffersForm : Form
    {
        private DataGridView dgvOffers;
        private Button btnRefresh;

        public ActiveOffersForm()
        {
            InitializeComponent();
            LoadOffers();
        }

        private void InitializeComponent()
        {
            Text = "Discount Offers";
            Size = new Size(920, 560);
            StartPosition = FormStartPosition.CenterParent;

            dgvOffers = new DataGridView
            {
                Location = new Point(10, 10),
                Size = new Size(880, 450),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            btnRefresh = new Button { Text = "Refresh", Location = new Point(10, 470), Size = new Size(80, 30) };
            btnRefresh.Click += (s, e) => LoadOffers();

            Controls.Add(dgvOffers);
            Controls.Add(btnRefresh);
        }

        private void LoadOffers()
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
SELECT
    O.OfferId,
    P.ProductName,
    P.Price AS OriginalPrice,
    O.DiscountPercent,
    CAST(P.Price - (P.Price * O.DiscountPercent / 100.0) AS DECIMAL(10,2)) AS DiscountedPrice,
    O.StartDate,
    O.EndDate,
    CASE WHEN GETDATE() BETWEEN O.StartDate AND O.EndDate THEN 'Active' ELSE 'Expired' END AS OfferStatus
FROM Offers O
INNER JOIN Products P ON P.ProductId = O.ProductId
ORDER BY OfferStatus DESC, O.EndDate DESC", conn);

            using var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            dgvOffers.DataSource = dt;
        }
    }
}
