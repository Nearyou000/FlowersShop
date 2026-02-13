using FlowersShop.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace FlowersShop.Data
{
    public class DatabaseHelper
    {
        private string connectionString;
        private string databasePath = "flowershop.db";

        public DatabaseHelper()
        {
            connectionString = $"Data Source={databasePath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Таблица товаров
                string createProductsTable = @"
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    Price DECIMAL(10,2) NOT NULL,
                    StockQuantity INTEGER NOT NULL,
                    Category TEXT,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
                )";

                // Таблица продаж
                string createSalesTable = @"
                CREATE TABLE IF NOT EXISTS Sales (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SaleDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    TotalAmount DECIMAL(10,2) NOT NULL,
                    ItemCount INTEGER NOT NULL
                )";

                // Таблица элементов продажи
                string createSaleItemsTable = @"
                CREATE TABLE IF NOT EXISTS SaleItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SaleId INTEGER NOT NULL,
                    ProductId INTEGER NOT NULL,
                    ProductName TEXT NOT NULL,
                    Quantity INTEGER NOT NULL,
                    UnitPrice DECIMAL(10,2) NOT NULL,
                    TotalPrice DECIMAL(10,2) NOT NULL,
                    FOREIGN KEY (SaleId) REFERENCES Sales(Id),
                    FOREIGN KEY (ProductId) REFERENCES Products(Id)
                )";

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = createProductsTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createSalesTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createSaleItemsTable;
                    command.ExecuteNonQuery();
                }
            }
        }

        // CRUD операции для товаров
        public int AddProduct(Product product)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO Products (Name, Description, Price, StockQuantity, Category) 
                               VALUES (@Name, @Description, @Price, @StockQuantity, @Category);
                               SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Description", product.Description);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
                    command.Parameters.AddWithValue("@Category", product.Category);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public void UpdateProduct(Product product)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"UPDATE Products SET Name = @Name, Description = @Description, 
                               Price = @Price, StockQuantity = @StockQuantity, Category = @Category 
                               WHERE Id = @Id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", product.Id);
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Description", product.Description);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
                    command.Parameters.AddWithValue("@Category", product.Category);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteProduct(int productId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Products WHERE Id = @Id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", productId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Products ORDER BY Name";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString(),
                            Price = Convert.ToDecimal(reader["Price"]),
                            StockQuantity = Convert.ToInt32(reader["StockQuantity"]),
                            Category = reader["Category"].ToString(),
                            CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                        });
                    }
                }
            }

            return products;
        }

        public List<Product> SearchProducts(string searchTerm)
        {
            var products = new List<Product>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Products WHERE Name LIKE @SearchTerm ORDER BY Name";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                Description = reader["Description"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                StockQuantity = Convert.ToInt32(reader["StockQuantity"]),
                                Category = reader["Category"].ToString(),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                            });
                        }
                    }
                }
            }

            return products;
        }

        public Product GetProductById(int id)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Products WHERE Id = @Id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Product
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                Description = reader["Description"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                StockQuantity = Convert.ToInt32(reader["StockQuantity"]),
                                Category = reader["Category"].ToString(),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        // Операции с продажами
        public int CreateSale(Sale sale, List<SaleItem> items)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Создаем продажу
                        string saleQuery = @"INSERT INTO Sales (SaleDate, TotalAmount, ItemCount) 
                                           VALUES (@SaleDate, @TotalAmount, @ItemCount);
                                           SELECT last_insert_rowid();";

                        int saleId;
                        using (var command = new SQLiteCommand(saleQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@SaleDate", sale.SaleDate);
                            command.Parameters.AddWithValue("@TotalAmount", sale.TotalAmount);
                            command.Parameters.AddWithValue("@ItemCount", sale.ItemCount);
                            saleId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        // Добавляем элементы продажи и обновляем остатки
                        foreach (var item in items)
                        {
                            string itemQuery = @"INSERT INTO SaleItems (SaleId, ProductId, ProductName, 
                                               Quantity, UnitPrice, TotalPrice) 
                                               VALUES (@SaleId, @ProductId, @ProductName, 
                                               @Quantity, @UnitPrice, @TotalPrice)";

                            using (var command = new SQLiteCommand(itemQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@SaleId", saleId);
                                command.Parameters.AddWithValue("@ProductId", item.ProductId);
                                command.Parameters.AddWithValue("@ProductName", item.ProductName);
                                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                                command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                                command.Parameters.AddWithValue("@TotalPrice", item.TotalPrice);
                                command.ExecuteNonQuery();
                            }

                            // Обновляем остатки товара
                            string updateStockQuery = @"UPDATE Products SET StockQuantity = StockQuantity - @Quantity 
                                                      WHERE Id = @ProductId";

                            using (var command = new SQLiteCommand(updateStockQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                                command.Parameters.AddWithValue("@ProductId", item.ProductId);
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return saleId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<Sale> GetAllSales()
        {
            var sales = new List<Sale>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Sales ORDER BY SaleDate DESC";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sales.Add(new Sale
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            SaleDate = Convert.ToDateTime(reader["SaleDate"]),
                            TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                            ItemCount = Convert.ToInt32(reader["ItemCount"])
                        });
                    }
                }
            }

            return sales;
        }

        public List<SaleItem> GetSaleItems(int saleId)
        {
            var items = new List<SaleItem>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM SaleItems WHERE SaleId = @SaleId";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SaleId", saleId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new SaleItem
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                SaleId = Convert.ToInt32(reader["SaleId"]),
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                ProductName = reader["ProductName"].ToString(),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                                TotalPrice = Convert.ToDecimal(reader["TotalPrice"])
                            });
                        }
                    }
                }
            }

            return items;
        }

        public decimal GetTotalRevenue()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COALESCE(SUM(TotalAmount), 0) FROM Sales";

                using (var command = new SQLiteCommand(query, connection))
                {
                    var result = command.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
        }

        public List<Product> GetLowStockProducts(int threshold = 10)
        {
            var products = new List<Product>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Products WHERE StockQuantity <= @Threshold ORDER BY StockQuantity";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Threshold", threshold);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                Description = reader["Description"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                StockQuantity = Convert.ToInt32(reader["StockQuantity"]),
                                Category = reader["Category"].ToString(),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                            });
                        }
                    }
                }
            }

            return products;
        }

        public void ExportSalesToCsv(string filePath, DateTime? startDate = null, DateTime? endDate = null)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Sales WHERE 1=1";

                if (startDate.HasValue)
                {
                    query += " AND SaleDate >= @StartDate";
                }

                if (endDate.HasValue)
                {
                    query += " AND SaleDate <= @EndDate";
                }

                query += " ORDER BY SaleDate";

                using (var command = new SQLiteCommand(query, connection))
                {
                    if (startDate.HasValue)
                        command.Parameters.AddWithValue("@StartDate", startDate.Value);

                    if (endDate.HasValue)
                        command.Parameters.AddWithValue("@EndDate", endDate.Value);

                    using (var reader = command.ExecuteReader())
                    using (var writer = new StreamWriter(filePath))
                    {
                        // Записываем заголовки
                        writer.WriteLine("ID;Дата продажи;Количество товаров;Общая сумма");

                        // Записываем данные
                        while (reader.Read())
                        {
                            writer.WriteLine($"{reader["Id"]};" +
                                           $"{Convert.ToDateTime(reader["SaleDate"]).ToString("yyyy-MM-dd HH:mm:ss")};" +
                                           $"{reader["ItemCount"]};" +
                                           $"{reader["TotalAmount"]}");
                        }
                    }
                }
            }
        }
    }
}
