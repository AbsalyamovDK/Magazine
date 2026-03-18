using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Magazine.Core.Models;

namespace Magazine.WebApi.Data
{
    /// <summary>
    /// Класс для работы с базой данных SQLite
    /// </summary>
    public class Database : IDisposable
    {
        // Константные строки с SQL-запросами
        private const string CREATE_TABLE_SQL = @"
            CREATE TABLE IF NOT EXISTS Products (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Definition TEXT,
                Price DECIMAL NOT NULL,
                Image TEXT
            )";
        
        private const string CREATE_INDEX_SQL = @"
            CREATE INDEX IF NOT EXISTS IX_Products_Id ON Products(Id)";
        
        private const string SELECT_ALL_SQL = "SELECT Id, Name, Definition, Price, Image FROM Products";
        
        private const string SELECT_BY_ID_SQL = "SELECT Id, Name, Definition, Price, Image FROM Products WHERE Id = $id";
        
        private const string INSERT_SQL = @"
            INSERT INTO Products (Id, Name, Definition, Price, Image)
            VALUES ($id, $name, $definition, $price, $image)";
        
        private const string UPDATE_SQL = @"
            UPDATE Products 
            SET Name = $name, Definition = $definition, Price = $price, Image = $image
            WHERE Id = $id";
        
        private const string DELETE_SQL = "DELETE FROM Products WHERE Id = $id";
        
        private const string SEARCH_SQL = @"
            SELECT Id, Name, Definition, Price, Image FROM Products 
            WHERE LOWER(Name) LIKE $searchTerm OR LOWER(Definition) LIKE $searchTerm
            LIMIT 1";

        private readonly string _connectionString;
        private SqliteConnection _connection;

        /// <summary>
        /// Конструктор - принимает путь к файлу БД
        /// </summary>
        public Database(string databasePath)
        {
            _connectionString = $"Data Source={databasePath}";
            InitializeDatabase();
        }

        /// <summary>
        /// Инициализация базы данных (создание таблицы и индекса)
        /// </summary>
        private void InitializeDatabase()
        {
            using var connection = GetConnection();
            connection.Open();
            
            // Создаем таблицу, если её нет
            using var createTableCmd = new SqliteCommand(CREATE_TABLE_SQL, connection);
            createTableCmd.ExecuteNonQuery();
            
            // Создаем индекс для поля Id
            using var createIndexCmd = new SqliteCommand(CREATE_INDEX_SQL, connection);
            createIndexCmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Получить соединение с БД
        /// </summary>
        private SqliteConnection GetConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        /// <summary>
        /// Получить все товары из БД
        /// </summary>
        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            
            using var connection = GetConnection();
            connection.Open();
            
            using var command = new SqliteCommand(SELECT_ALL_SQL, connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                products.Add(MapReaderToProduct(reader));
            }
            
            return products;
        }

        /// <summary>
        /// Получить товар по ID
        /// </summary>
        public Product? GetProductById(Guid id)
        {
            using var connection = GetConnection();
            connection.Open();
            
            using var command = new SqliteCommand(SELECT_BY_ID_SQL, connection);
            command.Parameters.AddWithValue("$id", id.ToString());
            
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                return MapReaderToProduct(reader);
            }
            
            return null;
        }

        /// <summary>
        /// Добавить новый товар
        /// </summary>
        public void AddProduct(Product product)
        {
            using var connection = GetConnection();
            connection.Open();
            
            using var command = new SqliteCommand(INSERT_SQL, connection);
            
            // Используем связывание переменных (безопасно от SQL-инъекций)
            command.Parameters.AddWithValue("$id", product.Id.ToString());
            command.Parameters.AddWithValue("$name", product.Name);
            command.Parameters.AddWithValue("$definition", product.Definition);
            command.Parameters.AddWithValue("$price", product.Price);
            command.Parameters.AddWithValue("$image", product.Image);
            
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Обновить существующий товар
        /// </summary>
        public void UpdateProduct(Product product)
        {
            using var connection = GetConnection();
            connection.Open();
            
            using var command = new SqliteCommand(UPDATE_SQL, connection);
            
            command.Parameters.AddWithValue("$id", product.Id.ToString());
            command.Parameters.AddWithValue("$name", product.Name);
            command.Parameters.AddWithValue("$definition", product.Definition);
            command.Parameters.AddWithValue("$price", product.Price);
            command.Parameters.AddWithValue("$image", product.Image);
            
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Удалить товар по ID
        /// </summary>
        public void DeleteProduct(Guid id)
        {
            using var connection = GetConnection();
            connection.Open();
            
            using var command = new SqliteCommand(DELETE_SQL, connection);
            command.Parameters.AddWithValue("$id", id.ToString());
            
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Поиск товара по строке
        /// </summary>
        public Product? SearchProduct(string searchTerm)
        {
            using var connection = GetConnection();
            connection.Open();
            
            using var command = new SqliteCommand(SEARCH_SQL, connection);
            command.Parameters.AddWithValue("$searchTerm, $searchTerm", $"%{searchTerm.ToLower()}%");
            
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                return MapReaderToProduct(reader);
            }
            
            return null;
        }

        /// <summary>
        /// Преобразовать строку из БД в объект Product
        /// </summary>
        private Product MapReaderToProduct(SqliteDataReader reader)
        {
            return new Product
            {
                Id = Guid.Parse(reader.GetString(0)),
                Name = reader.GetString(1),
                Definition = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Price = reader.GetDecimal(3),
                Image = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
            };
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}