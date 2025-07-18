using WarehouseManagementSystem.Forms;

namespace WarehouseManagementSystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application error: {ex.Message}", "Fatal Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}