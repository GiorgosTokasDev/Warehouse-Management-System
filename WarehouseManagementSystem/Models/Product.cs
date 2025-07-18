using System;

namespace WarehouseManagementSystem.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public decimal UnitPrice { get; set; }
        public int MinStockLevel { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}