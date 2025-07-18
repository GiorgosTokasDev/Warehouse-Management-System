using System;

namespace WarehouseManagementSystem.Models
{
    public class StockTransaction
    {
        public int TransactionId { get; set; }
        public int ProductId { get; set; }
        public StockTransactionType TransactionType { get; set; }
        public int Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Reference { get; set; }
        public string Notes { get; set; }
    }

    public enum StockTransactionType
    {
        In,
        Out
    }
}