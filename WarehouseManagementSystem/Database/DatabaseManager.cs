using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace WarehouseManagementSystem.Database
{
    public class DatabaseManager
    {
        private string connectionString;

        public DatabaseManager()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            Console.WriteLine($"Current directory: {currentDirectory}");

            var builder = new ConfigurationBuilder()
                .SetBasePath(currentDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("secrets.json", optional: true, reloadOnChange: true);

            IConfiguration config = builder.Build();

            string baseConnection = config.GetConnectionString("WarehouseDB");
            string password = config["DbPassword"] ??
                              Environment.GetEnvironmentVariable("DB_PASSWORD");

            connectionString = $"{baseConnection}Pwd={password};";

            Console.WriteLine($"Connection string: {connectionString.Replace(password, "*****")}");
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        public void InitializeDatabase()
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();

                    var createProductsTable = @"
                        CREATE TABLE IF NOT EXISTS Products (
                            ProductId INT AUTO_INCREMENT PRIMARY KEY,
                            ProductCode VARCHAR(50) UNIQUE NOT NULL,
                            ProductName VARCHAR(255) NOT NULL,
                            Description TEXT,
                            Category VARCHAR(100),
                            UnitPrice DECIMAL(10,2),
                            MinStockLevel INT DEFAULT 0,
                            CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
                        )";

                    var createStockTable = @"
                        CREATE TABLE IF NOT EXISTS Stock (
                            StockId INT AUTO_INCREMENT PRIMARY KEY,
                            ProductId INT,
                            Quantity INT NOT NULL,
                            FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
                        )";

                    var createTransactionsTable = @"
                        CREATE TABLE IF NOT EXISTS StockTransactions (
                            TransactionId INT AUTO_INCREMENT PRIMARY KEY,
                            ProductId INT,
                            TransactionType ENUM('IN', 'OUT') NOT NULL,
                            Quantity INT NOT NULL,
                            TransactionDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                            Reference VARCHAR(255),
                            Notes TEXT,
                            FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
                        )";

                    ExecuteCommand(createProductsTable, conn);
                    ExecuteCommand(createStockTable, conn);
                    ExecuteCommand(createTransactionsTable, conn);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Database initialization failed: {ex.Message}", "Database Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExecuteCommand(string sql, MySqlConnection conn)
        {
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
