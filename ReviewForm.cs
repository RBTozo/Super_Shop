using System;
using System.Windows.Forms;

namespace SuperShop
{
    public class ReviewForm : Form
    {
        private int productId;
        private NumericUpDown nudRating;
        private TextBox txtComment;
        private Button btnSubmit, btnCancel;

        public ReviewForm(int productId)
        {
            this.productId = productId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Submit Review";
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;

            Label lbl = new Label { Text = "Rating (1-5):", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            nudRating = new NumericUpDown { Location = new System.Drawing.Point(120, 18), Minimum = 1, Maximum = 5, Value = 5 };

            Label lbl2 = new Label { Text = "Comment:", Location = new System.Drawing.Point(20, 60), AutoSize = true };
            txtComment = new TextBox { Location = new System.Drawing.Point(20, 85), Size = new System.Drawing.Size(340, 110), Multiline = true };

            btnSubmit = new Button { Text = "Submit", Location = new System.Drawing.Point(200, 210), Size = new System.Drawing.Size(75, 30) };
            btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(285, 210), Size = new System.Drawing.Size(75, 30) };

            btnSubmit.Click += BtnSubmit_Click;
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.Add(lbl);
            this.Controls.Add(nudRating);
            this.Controls.Add(lbl2);
            this.Controls.Add(txtComment);
            this.Controls.Add(btnSubmit);
            this.Controls.Add(btnCancel);
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                int rating = (int)nudRating.Value;
                string comment = txtComment.Text ?? string.Empty;
                DatabaseHelper.InsertReview(Session.UserId, productId, rating, comment);
                MessageBox.Show("Thank you for your review!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error submitting review: " + ex.Message);
            }
        }
    }
}
