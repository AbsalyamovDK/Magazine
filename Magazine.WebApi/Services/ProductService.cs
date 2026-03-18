using System;
using System.Collections.Generic;
using System.Linq;
using Magazine.Core.Models;
using Magazine.Core.Services;
using Magazine.WebApi.Data;
using Microsoft.Extensions.Configuration;

namespace Magazine.WebApi.Services
{
    /// <summary>
    /// Реализация сервиса товаров с использованием SQLite
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly Database _database;

        /// <summary>
        /// Конструктор - принимает конфигурацию и создает подключение к БД
        /// </summary>
        public ProductService(IConfiguration configuration)
        {
            // Получаем путь к файлу БД из конфигурации
            var databasePath = configuration["Database:FilePath"] 
                ?? throw new InvalidOperationException("Database:FilePath не найден в конфигурации");
            
            // Создаем экземпляр Database для работы с SQLite
            _database = new Database(databasePath);
        }

        /// <summary>
        /// Добавление нового товара
        /// </summary>
        public Product Add(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            // Проверяем, существует ли уже товар с таким ID
            var existing = _database.GetProductById(product.Id);
            if (existing != null)
                throw new InvalidOperationException($"Товар с Id {product.Id} уже существует");

            // Если ID пустой - генерируем новый
            if (product.Id == Guid.Empty)
                product.Id = Guid.NewGuid();

            // Добавляем в базу данных
            _database.AddProduct(product);
            
            return product;
        }

        /// <summary>
        /// Удаление товара по ID
        /// </summary>
        public Product? Remove(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id не может быть пустым");

            // Сначала получаем товар (чтобы вернуть его после удаления)
            var product = _database.GetProductById(id);
            
            if (product != null)
            {
                _database.DeleteProduct(id);
            }
            
            return product;
        }

        /// <summary>
        /// Редактирование товара
        /// </summary>
        public Product Edit(Product updatedProduct)
        {
            if (updatedProduct == null)
                throw new ArgumentNullException(nameof(updatedProduct));

            // Проверяем, существует ли товар
            var existing = _database.GetProductById(updatedProduct.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Товар с Id {updatedProduct.Id} не найден");

            // Обновляем в базе данных
            _database.UpdateProduct(updatedProduct);
            
            return updatedProduct;
        }

        /// <summary>
        /// Поиск товара по строке
        /// </summary>
        public Product? Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Поисковый запрос не может быть пустым");

            return _database.SearchProduct(searchTerm);
        }

        /// <summary>
        /// Получение всех товаров
        /// </summary>
        public IEnumerable<Product> GetAll()
        {
            return _database.GetAllProducts();
        }

        /// <summary>
        /// Получение товара по ID
        /// </summary>
        public Product? GetById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id не может быть пустым");

            return _database.GetProductById(id);
        }
    }
}