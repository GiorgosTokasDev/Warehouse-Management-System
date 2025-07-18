-- Create database  
CREATE DATABASE IF NOT EXISTS warehouse_db;  
USE warehouse_db;  
  
-- Products table  
CREATE TABLE IF NOT EXISTS Products (  
    ProductId INT AUTO_INCREMENT PRIMARY KEY,  
    ProductCode VARCHAR(50) UNIQUE NOT NULL,  
    ProductName VARCHAR(255) NOT NULL,  
    Description TEXT,  
    Category VARCHAR(100),  
    UnitPrice DECIMAL(10,2),  
    MinStockLevel INT DEFAULT 0,  
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP  
);  
  
-- Stock table  
CREATE TABLE IF NOT EXISTS Stock (  
    StockId INT AUTO_INCREMENT PRIMARY KEY,  
    ProductId INT,  
    Quantity INT NOT NULL,  
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId)  
);  
  
-- StockTransactions table  
CREATE TABLE IF NOT EXISTS StockTransactions (  
    TransactionId INT AUTO_INCREMENT PRIMARY KEY,  
    ProductId INT,  
    TransactionType ENUM('IN', 'OUT') NOT NULL,  
    Quantity INT NOT NULL,  
    TransactionDate DATETIME DEFAULT CURRENT_TIMESTAMP,  
    Reference VARCHAR(255),  
    Notes TEXT,  
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId)  
);  
  
-- Sample data  
INSERT INTO Products (ProductCode, ProductName, Category, UnitPrice, MinStockLevel)  
VALUES  
    ('P1001', 'Laptop', 'Electronics', 899.99, 5),  
    ('P1002', 'Desk Chair', 'Furniture', 129.50, 10),  
    ('P1003', 'Wireless Mouse', 'Accessories', 24.99, 20);  
  
INSERT INTO Stock (ProductId, Quantity)  
VALUES  
    (1, 15),  
    (2, 8),  
    (3, 25);  
  
INSERT INTO StockTransactions (ProductId, TransactionType, Quantity, Reference)  
VALUES  
    (1, 'IN', 20, 'PO-2023-001'),  
    (2, 'IN', 15, 'PO-2023-002'),  
