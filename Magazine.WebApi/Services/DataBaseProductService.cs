using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Magazine.Core.Models;
using Magazine.Core.Services;
using Magazine.WebApi.Data;

namespace Magazine.WebApi.Services
{
    /// <summary>
    /// Реализация сервиса товаров с использованием Entity Framework
    /// </summary>
    public class DataBaseProductService : IProductService
    {
        private readonly ApplicationContext _context;

        /// <summary>
        /// Конструктор - принимает контекст БД
        /// </summary>
        public DataBaseProductService(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Добавление нового товара
        /// </summary>
        public Product Add(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            // Проверяем, существует ли товар с таким ID
            if (_context.Products.Any(p => p.Id == product.Id))
                throw new InvalidOperationException($"Товар с Id {product.Id} уже существует");

            // Если ID пустой - генерируем новый
            if (product.Id == Guid.Empty)
                product.Id = Guid.NewGuid();

            // Устанавливаем дату создания
            product.CreatedAt = DateTime.UtcNow;

            // Добавляем в контекст
            _context.Products.Add(product);
            
            // Сохраняем изменения в БД
            _context.SaveChanges();
            
            return product;
        }

        /// <summary>
        /// Удаление товара по ID
        /// </summary>
        public Product? Remove(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id не может быть пустым");

            // Ищем товар
            var product = _context.Products.Find(id);
            
            if (product != null)
            {
                // Удаляем из контекста
                _context.Products.Remove(product);
                // Сохраняем изменения
                _context.SaveChanges();
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

            // Ищем существующий товар
            var existingProduct = _context.Products.Find(updatedProduct.Id);
            
            if (existingProduct == null)
                throw new KeyNotFoundException($"Товар с Id {updatedProduct.Id} не найден");

            // Обновляем поля
            existingProduct.Name = updatedProduct.Name;
            existingProduct.Definition = updatedProduct.Definition;
            existingProduct.Price = updatedProduct.Price;
            existingProduct.Image = updatedProduct.Image;
            // CreatedAt НЕ обновляем - дата создания не меняется

            // Сохраняем изменения
            _context.SaveChanges();
            
            return existingProduct;
        }

        /// <summary>
        /// Поиск товара по строке
        /// </summary>
        public Product? Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Поисковый запрос не может быть пустым");

            var normalizedSearchTerm = searchTerm.ToLower().Trim();
            
            // Используем LINQ для поиска
            return _context.Products
                .Where(p => p.Name.ToLower().Contains(normalizedSearchTerm) ||
                            p.Definition.ToLower().Contains(normalizedSearchTerm))
                .FirstOrDefault();
        }

        /// <summary>
        /// Получение всех товаров (отсортированных по дате создания)
        /// </summary>
        public IEnumerable<Product> GetAll()
        {
            return _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
        }

        /// <summary>
        /// Получение товара по ID
        /// </summary>
        public Product? GetById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id не может быть пустым");

            return _context.Products.Find(id);
        }
    }
}