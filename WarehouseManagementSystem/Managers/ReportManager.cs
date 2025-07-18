using System.Data;
using MySql.Data.MySqlClient;
using WarehouseManagementSystem.Database;

namespace WarehouseManagementSystem.Managers
{
    public class ReportManager
    {
        private DatabaseManager dbManager;

        public ReportManager(DatabaseManager dbManager)
        {
            this.dbManager = dbManager;
        }

        public DataTable GetInventoryReport()
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT p.ProductCode, p.ProductName, p.Category, s.Quantity, 
                           p.UnitPrice, (s.Quantity * p.UnitPrice) as TotalValue,
                           p.MinStockLevel,
                           CASE WHEN s.Quantity <= p.MinStockLevel THEN 'Low Stock' ELSE 'Adequate' END as StockStatus
                           FROM Products p
                           JOIN Stock s ON p.ProductId = s.ProductId
                           ORDER BY TotalValue DESC";

                var adapter = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public DataTable GetStockMovementReport()
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT DATE(st.TransactionDate) as Date, p.ProductCode, p.ProductName,
                           SUM(CASE WHEN st.TransactionType = 'IN' THEN st.Quantity ELSE 0 END) as StockIn,
                           SUM(CASE WHEN st.TransactionType = 'OUT' THEN st.Quantity ELSE 0 END) as StockOut,
                           SUM(CASE WHEN st.TransactionType = 'IN' THEN st.Quantity ELSE -st.Quantity END) as NetMovement
                           FROM StockTransactions st
                           JOIN Products p ON st.ProductId = p.ProductId
                           WHERE st.TransactionDate >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
                           GROUP BY DATE(st.TransactionDate), p.ProductId
                           ORDER BY Date DESC, p.ProductName";

                var adapter = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public DataTable GetLowStockReport()
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT p.ProductCode, p.ProductName, p.Category, s.Quantity, 
                           p.MinStockLevel, (p.MinStockLevel - s.Quantity) as ShortfallQuantity,
                           p.UnitPrice, ((p.MinStockLevel - s.Quantity) * p.UnitPrice) as ReorderValue
                           FROM Products p
                           JOIN Stock s ON p.ProductId = s.ProductId
                           WHERE s.Quantity <= p.MinStockLevel
                           ORDER BY ShortfallQuantity DESC";

                var adapter = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }
    }
}
