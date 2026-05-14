using System;
using System.Windows.Forms;

namespace SuperShop
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            // Create the database and schema before testing a live connection.
            try
            {
                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception initEx)
            {
                MessageBox.Show(
                    "Cannot initialize the database.\n\n" +
                    initEx.Message +
                    "\n\nCheck that SQL Server LocalDB or SQL Server Express is installed, then run AUTO_SETUP.bat again.",
                    "Database Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // Verify database connectivity after initialization.
            if (!DatabaseHelper.VerifyConnection(out var err))
            {
                MessageBox.Show($"Cannot connect to database:\n{err}", "Database Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new LoginForm());
        }
    }
}
