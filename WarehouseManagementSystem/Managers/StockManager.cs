using System.Data;
using MySql.Data.MySqlClient;
using WarehouseManagementSystem.Database;
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Managers
{
    public class StockManager
    {
        private DatabaseManager dbManager;

        public StockManager(DatabaseManager dbManager)
        {
            this.dbManager = dbManager;
        }

        public void RecordStockTransaction(StockTransaction transaction)
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                using (var dbTransaction = conn.BeginTransaction())
                {
                    try
                    {
                        var sql = @"INSERT INTO StockTransactions (ProductId, TransactionType, Quantity, Reference, Notes) 
                                   VALUES (@ProductId, @TransactionType, @Quantity, @Reference, @Notes)";

                        using (var cmd = new MySqlCommand(sql, conn, dbTransaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductId", transaction.ProductId);
                            cmd.Parameters.AddWithValue("@TransactionType", transaction.TransactionType.ToString().ToUpper());
                            cmd.Parameters.AddWithValue("@Quantity", transaction.Quantity);
                            cmd.Parameters.AddWithValue("@Reference", transaction.Reference ?? "");
                            cmd.Parameters.AddWithValue("@Notes", transaction.Notes ?? "");
                            cmd.ExecuteNonQuery();
                        }

                        var quantityChange = transaction.TransactionType == StockTransactionType.In
                            ? transaction.Quantity : -transaction.Quantity;

                        var updateStock = "UPDATE Stock SET Quantity = Quantity + @QuantityChange WHERE ProductId = @ProductId";
                        using (var cmd = new MySqlCommand(updateStock, conn, dbTransaction))
                        {
                            cmd.Parameters.AddWithValue("@QuantityChange", quantityChange);
                            cmd.Parameters.AddWithValue("@ProductId", transaction.ProductId);
                            cmd.ExecuteNonQuery();
                        }

                        dbTransaction.Commit();
                    }
                    catch
                    {
                        dbTransaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public DataTable GetStockTransactions()
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT st.TransactionId, p.ProductCode, p.ProductName, st.TransactionType, 
                           st.Quantity, st.TransactionDate, st.Reference, st.Notes
                           FROM StockTransactions st
                           JOIN Products p ON st.ProductId = p.ProductId
                           ORDER BY st.TransactionDate DESC";

                var adapter = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public DataTable GetCurrentInventory()
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT p.ProductCode, p.ProductName, p.Category, s.Quantity, 
                           p.MinStockLevel, p.UnitPrice,
                           CASE WHEN s.Quantity <= p.MinStockLevel THEN 'Low Stock' ELSE 'OK' END as Status
                           FROM Products p
                           JOIN Stock s ON p.ProductId = s.ProductId
                           ORDER BY p.ProductName";

                var adapter = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }
    }
}
