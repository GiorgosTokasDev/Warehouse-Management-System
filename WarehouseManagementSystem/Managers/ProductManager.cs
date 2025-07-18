using System.Data;
using MySql.Data.MySqlClient;
using WarehouseManagementSystem.Database;
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Managers
{
    public class ProductManager
    {
        private DatabaseManager dbManager;

        public ProductManager(DatabaseManager dbManager)
        {
            this.dbManager = dbManager;
        }

        public DataTable GetAllProducts()
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = "SELECT * FROM Products ORDER BY ProductName";
                var adapter = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public Product GetProduct(int productId)
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = "SELECT * FROM Products WHERE ProductId = @ProductId";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductId", productId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Product
                            {
                                ProductId = reader.GetInt32("ProductId"),
                                ProductCode = reader.GetString("ProductCode"),
                                ProductName = reader.GetString("ProductName"),
                                Description = reader.IsDBNull("Description") ? "" : reader.GetString("Description"),
                                Category = reader.IsDBNull("Category") ? "" : reader.GetString("Category"),
                                UnitPrice = reader.GetDecimal("UnitPrice"),
                                MinStockLevel = reader.GetInt32("MinStockLevel"),
                                CreatedDate = reader.GetDateTime("CreatedDate")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public void AddProduct(Product product)
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = @"INSERT INTO Products (ProductCode, ProductName, Description, Category, UnitPrice, MinStockLevel) 
                           VALUES (@ProductCode, @ProductName, @Description, @Category, @UnitPrice, @MinStockLevel)";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductCode", product.ProductCode);
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@Description", product.Description);
                    cmd.Parameters.AddWithValue("@Category", product.Category);
                    cmd.Parameters.AddWithValue("@UnitPrice", product.UnitPrice);
                    cmd.Parameters.AddWithValue("@MinStockLevel", product.MinStockLevel);
                    cmd.ExecuteNonQuery();
                    var productId = (int)cmd.LastInsertedId;

                    var stockSql = "INSERT INTO Stock (ProductId, Quantity) VALUES (@ProductId, 0)";
                    using (var stockCmd = new MySqlCommand(stockSql, conn))
                    {
                        stockCmd.Parameters.AddWithValue("@ProductId", productId);
                        stockCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void UpdateProduct(Product product)
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                var sql = @"UPDATE Products SET ProductCode = @ProductCode, ProductName = @ProductName, 
                           Description = @Description, Category = @Category, UnitPrice = @UnitPrice, 
                           MinStockLevel = @MinStockLevel WHERE ProductId = @ProductId";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductId", product.ProductId);
                    cmd.Parameters.AddWithValue("@ProductCode", product.ProductCode);
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@Description", product.Description);
                    cmd.Parameters.AddWithValue("@Category", product.Category);
                    cmd.Parameters.AddWithValue("@UnitPrice", product.UnitPrice);
                    cmd.Parameters.AddWithValue("@MinStockLevel", product.MinStockLevel);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteProduct(int productId)
        {
            using (var conn = dbManager.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var deleteStock = "DELETE FROM Stock WHERE ProductId = @ProductId";
                        using (var cmd = new MySqlCommand(deleteStock, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductId", productId);
                            cmd.ExecuteNonQuery();
                        }

                        var deleteTransactions = "DELETE FROM StockTransactions WHERE ProductId = @ProductId";
                        using (var cmd = new MySqlCommand(deleteTransactions, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductId", productId);
                            cmd.ExecuteNonQuery();
                        }

                        var deleteProduct = "DELETE FROM Products WHERE ProductId = @ProductId";
                        using (var cmd = new MySqlCommand(deleteProduct, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductId", productId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
