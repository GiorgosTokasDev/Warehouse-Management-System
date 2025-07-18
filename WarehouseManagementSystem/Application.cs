using WarehouseManagementSystem.Forms;

namespace WarehouseManagementSystem
{
    internal static class Application
    {
        [STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                System.Windows.Forms.Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application error: {ex.Message}", "Fatal Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}