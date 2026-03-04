using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Magazine.Core.Models;
using Magazine.Core.Services;
using Microsoft.Extensions.Configuration;

namespace Magazine.WebApi.Services
{
    public class ProductService : IProductService
    {
        // Словарь для хранения данных в памяти (ключ - Id товара)
        private readonly Dictionary<Guid, Product> _products;
        
        // Путь к файлу базы данных из конфигурации
        private readonly string _filePath;
        
        // Мьютекс для потокобезопасной записи в файл
        private static readonly Mutex _fileMutex = new Mutex();

        // Конструктор - принимает IConfiguration
        public ProductService(IConfiguration configuration)
        {
            // Получаем путь к файлу из конфигурации
            _filePath = configuration["DataBaseFilePath"] 
                ?? throw new InvalidOperationException("DataBaseFilePath не найден в конфигурации");
            
            // Инициализируем словарь
            _products = new Dictionary<Guid, Product>();
            
            // Загружаем данные из файла при создании сервиса
            InitFromFile();
        }

        /// <summary>
        /// Загрузка данных из файла (десериализация)
        /// </summary>
        private void InitFromFile()
        {
            try
            {
                // Проверяем, существует ли файл
                if (File.Exists(_filePath))
                {
                    // Читаем весь текст из файла
                    string jsonText = File.ReadAllText(_filePath);
                    
                    // Десериализуем JSON в список продуктов
                    var products = JsonSerializer.Deserialize<List<Product>>(jsonText);
                    
                    // Очищаем словарь и заполняем из списка
                    _products.Clear();
                    if (products != null)
                    {
                        foreach (var product in products)
                        {
                            _products[product.Id] = product;
                        }
                    }
                }
                else
                {
                    // Если файла нет - создаем пустой словарь
                    Console.WriteLine($"Файл {_filePath} не найден. Будет создан новый при сохранении.");
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не прерываем работу
                Console.WriteLine($"Ошибка при загрузке из файла: {ex.Message}");
            }
        }

        /// <summary>
        /// Сохранение данных в файл (сериализация)
        /// </summary>
        private void WriteToFile()
        {
            try
            {
                // Захватываем мьютекс для потокобезопасной записи
                _fileMutex.WaitOne();
                
                try
                {
                    // Преобразуем словарь в список для сериализации
                    var products = _products.Values.ToList();
                    
                    // Сериализуем список в JSON (с красивым форматированием)
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string jsonText = JsonSerializer.Serialize(products, options);
                    
                    // Записываем в файл
                    File.WriteAllText(_filePath, jsonText);
                }
                finally
                {
                    // Освобождаем мьютекс в любом случае
                    _fileMutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при записи в файл: {ex.Message}");
                throw; // Пробрасываем исключение дальше
            }
        }

        public Product Add(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            // Проверяем, существует ли уже товар с таким Id
            if (_products.ContainsKey(product.Id))
                throw new InvalidOperationException($"Товар с Id {product.Id} уже существует");

            // Если Id пустой - генерируем новый
            if (product.Id == Guid.Empty)
                product.Id = Guid.NewGuid();

            // Добавляем в словарь
            _products[product.Id] = product;
            
            // Сохраняем изменения на диск
            WriteToFile();
            
            return product;
        }

        public Product? Remove(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id не может быть пустым");

            // Пытаемся удалить из словаря
            if (_products.TryGetValue(id, out var product))
            {
                _products.Remove(id);
                
                // Сохраняем изменения на диск
                WriteToFile();
                
                return product;
            }
            
            return null; // Товар не найден
        }

        public Product Edit(Product updatedProduct)
        {
            if (updatedProduct == null)
                throw new ArgumentNullException(nameof(updatedProduct));

            // Проверяем, существует ли товар
            if (!_products.ContainsKey(updatedProduct.Id))
                throw new KeyNotFoundException($"Товар с Id {updatedProduct.Id} не найден");

            // Обновляем данные в словаре
            _products[updatedProduct.Id] = updatedProduct;
            
            // Сохраняем изменения на диск
            WriteToFile();
            
            return updatedProduct;
        }

        public Product? Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Поисковый запрос не может быть пустым");

            var normalizedSearchTerm = searchTerm.ToLower().Trim();
            
            // Ищем по всем товарам в словаре
            return _products.Values.FirstOrDefault(p => 
                (p.Name?.ToLower().Contains(normalizedSearchTerm) ?? false) ||
                (p.Definition?.ToLower().Contains(normalizedSearchTerm) ?? false)
            );
        }

        public IEnumerable<Product> GetAll()
        {
            // Возвращаем копию всех значений словаря
            return _products.Values.ToList();
        }

        public Product? GetById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id не может быть пустым");

            // Пытаемся получить из словаря
            _products.TryGetValue(id, out var product);
            return product;
        }
    }
}