using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Magazine.Core.Models;
using Magazine.Core.Services;
using Magazine.WebApi.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace Magazine.Tests
{
    /// <summary>
    /// Класс для тестирования ProductService
    /// </summary>
    [TestFixture]  // Атрибут NUnit - указывает, что класс содержит тесты
    public class TestsProductService
    {
        private string _testFilePath;           // Путь к временному файлу для тестов
        private IConfiguration _configuration;  // Конфигурация для тестов
        private List<Product> _testProducts;    // Тестовые данные

        /// <summary>
        /// Метод, выполняющийся перед КАЖДЫМ тестом
        /// Подготавливает чистую среду для тестирования
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Создаем уникальное имя для временного файла
            // Guid.NewGuid() гарантирует уникальность
            _testFilePath = $"test_database_{Guid.NewGuid()}.json";
            
            // Создаем конфигурацию для тестов
            // inMemorySettings - словарь с настройками
            var inMemorySettings = new Dictionary<string, string>
            {
                {"DataBaseFilePath", _testFilePath}  // Путь к временному файлу
            };
            
            // Создаем объект конфигурации из словаря
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
            
            // Создаем тестовые данные
            _testProducts = new List<Product>
            {
                new Product
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Тестовый товар 1",
                    Definition = "Описание 1",
                    Price = 100.50m,
                    Image = "test1.jpg"
                },
                new Product
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Тестовый товар 2",
                    Definition = "Описание 2",
                    Price = 200.75m,
                    Image = "test2.jpg"
                },
                new Product
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Samsung телефон",
                    Definition = "Описание Samsung",
                    Price = 500.00m,
                    Image = "samsung.jpg"
                }
            };
        }

        /// <summary>
        /// Метод, выполняющийся после КАЖДОГО теста
        /// Очищает временные файлы
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            // Удаляем временный файл, если он существует
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        /// <summary>
        /// Тест 1: Проверка создания сервиса
        /// </summary>
        [Test]  // Атрибут NUnit - обозначает тестовый метод
        public void Constructor_WhenCalled_CreatesService()
        {
            // ACT - выполняем действие
            var service = new ProductService(_configuration);
            
            // ASSERT - проверяем результат
            Assert.That(service, Is.Not.Null);  // Сервис не должен быть null
        }

        /// <summary>
        /// Тест 2: Добавление товара (позитивный)
        /// </summary>
        [Test]
        public void Add_ValidProduct_AddsProductAndSavesToFile()
        {
            // ARRANGE - подготавливаем
            var service = new ProductService(_configuration);
            var newProduct = new Product
            {
                Name = "Новый товар",
                Definition = "Новое описание",
                Price = 300.00m,
                Image = "new.jpg"
            };

            // ACT - выполняем
            var result = service.Add(newProduct);
            
            // ASSERT - проверяем результат в памяти
            Assert.That(result, Is.Not.Null);                    // Результат не null
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty)); // ID должен быть заполнен
            Assert.That(result.Name, Is.EqualTo("Новый товар")); // Имя совпадает
            
            // Проверяем, что товар добавился в словарь
            var allProducts = service.GetAll();
            Assert.That(allProducts.Count(), Is.EqualTo(1));     // Один товар в списке
            
            // Проверяем, что данные сохранились в файл
            Assert.That(File.Exists(_testFilePath), Is.True);    // Файл создался
            
            // Читаем файл и проверяем содержимое
            var jsonText = File.ReadAllText(_testFilePath);
            var productsFromFile = JsonSerializer.Deserialize<List<Product>>(jsonText);
            Assert.That(productsFromFile!.Count, Is.EqualTo(1)); // В файле один товар
        }

        /// <summary>
        /// Тест 3: Добавление null товара (негативный)
        /// </summary>
        [Test]
        public void Add_NullProduct_ThrowsArgumentNullException()
        {
            // ARRANGE
            var service = new ProductService(_configuration);
            
            // ACT & ASSERT - проверяем, что выбрасывается исключение
            Assert.That(() => service.Add(null!), 
                Throws.ArgumentNullException);  // Ожидаем ArgumentNullException
        }

        /// <summary>
        /// Тест 4: Добавление товара с существующим ID
        /// </summary>
        [Test]
        public void Add_ProductWithExistingId_ThrowsInvalidOperationException()
        {
            // ARRANGE
            var service = new ProductService(_configuration);
            
            // Сначала добавляем товар
            var product1 = new Product
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Первый товар",
                Definition = "Описание",
                Price = 100m
            };
            service.Add(product1);
            
            // Пытаемся добавить товар с тем же ID
            var product2 = new Product
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), // Тот же ID!
                Name = "Другой товар",
                Definition = "Другое описание",
                Price = 200m
            };
            
            // ACT & ASSERT
            Assert.That(() => service.Add(product2), 
                Throws.InvalidOperationException  // Ожидаем InvalidOperationException
                    .With.Message.Contains("уже существует"));  // Проверяем сообщение
        }

        /// <summary>
        /// Тест 5: Удаление существующего товара
        /// </summary>
        [Test]
        public void Remove_ExistingId_RemovesProductAndSavesToFile()
        {
            // ARRANGE
            var service = new ProductService(_configuration);
            var product = _testProducts[0];
            service.Add(product);  // Добавляем товар
            
            // ACT
            var removed = service.Remove(product.Id);
            
            // ASSERT
            Assert.That(removed, Is.Not.Null);                     // Удаленный товар не null
            Assert.That(removed!.Id, Is.EqualTo(product.Id));      // ID совпадает
            
            var allProducts = service.GetAll();
            Assert.That(allProducts.Count(), Is.EqualTo(0));       // Список пуст
            
            // Проверяем файл
            var jsonText = File.ReadAllText(_testFilePath);
            var productsFromFile = JsonSerializer.Deserialize<List<Product>>(jsonText);
            Assert.That(productsFromFile!.Count, Is.EqualTo(0));   // В файле пусто
        }

        /// <summary>
        /// Тест 6: Удаление несуществующего товара
        /// </summary>
        [Test]
        public void Remove_NonExistingId_ReturnsNull()
        {
            // ARRANGE
            var service = new ProductService(_configuration);
            var nonExistingId = Guid.NewGuid();
            
            // ACT
            var result = service.Remove(nonExistingId);
            
            // ASSERT
            Assert.That(result, Is.Null);  // Должен вернуть null
        }

        /// <summary>
        /// Тест 7: Удаление с пустым ID
        /// </summary>
        [Test]
        public void Remove_EmptyId_ThrowsArgumentException()
        {
            // ARRANGE
            var service = new ProductService(_configuration);
            
            // ACT & ASSERT
            Assert.That(() => service.Remove(Guid.Empty), 
                Throws.ArgumentException);  // Ожидаем ArgumentException
        }

        /// <summary>
        /// Тест 8: Редактирование существующего товара
        /// </summary>
        [Test]
        public void Edit_ExistingProduct_UpdatesProductAndSavesToFile()
        {
            // ARRANGE
            var service = new ProductService(_configuration);
            var originalProduct = _testProducts[0];
            service.Add(originalProduct);
            
            // Создаем обновленную версию
            var updatedProduct = new Product
            {
                Id = originalProduct.Id,  // Тот же ID
                Name = "Обновленное название",
                Definition = "Обновленное описание",
                Price = 999.99m,
                Image = "updated.jpg"
            };
            
            // ACT
            var result = service.Edit(updatedProduct);
            
            // ASSERT
            Assert.That(result.Name, Is.EqualTo("Обновленное название"));
            Assert.That(result.Definition, Is.EqualTo("Обновленное описание"));
            Assert.That(result.Price, Is.EqualTo(999.99m));
            
            // Проверяем, что изменения сохранились
            var productFromService = service.GetById(originalProduct.Id);
            Assert.That(productFromService!.Name, Is.EqualTo("Обновленное название"));
            
            // Проверяем файл
            var jsonText = File.ReadAllText(_testFilePath);
            var productsFromFile = JsonSerializer.Deserialize<List<Product>>(jsonText);
            var fileProduct = productsFromFile!.First(p => p.Id == originalProduct.Id);
            Assert.That(fileProduct.Name, Is.EqualTo("Обновленное название"));
        }

        /// <summary>
        /// Тест 9: Редактирование несуществующего товара
        /// </summary>
        [Test]
        public void Edit_NonExistingProduct_ThrowsKeyNotFoundException()
        {
            // ARRANGE
            var service = new ProductService(_configuration);
            var nonExistingProduct = _testProducts[0];  // Не добавляли в сервис
            
            // ACT & ASSERT
            Assert.That(() => service.Edit(nonExistingProduct), 
                Throws.TypeOf<KeyNotFoundException>());  // Ожидаем KeyNotFoundException
        }

        /// <summary>
        /// Тест 10: Поиск существующего товара
        /// </summary>
        [Test]
        public void Search_ExistingTerm_ReturnsProduct()
        {
            // ARRANGE
            var service = new ProductService(_configuration);
            service.Add(_testProducts[0]);
            service.Add(_testProducts[1]);
            service.Add(_testProducts[2]);  // Товар с "Samsung"
            
            // ACT
            var result = service.Search("Samsung");
            
            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Does.Contain("Samsung"));  // Имя содержит "Samsung"
        }

        /// <summary>
        /// Тест 11: Поиск несуществующего товара
        /// </summary>
        [Test]
        public void Search_NonExistingTerm_ReturnsNull()
        {
            // ARRANGE
            var service = new ProductService(_configuration);
            service.Add(_testProducts[0]);
            service.Add(_testProducts[1]);
            
            // ACT
            var result = service.Search("NonExistent");
            
            // ASSERT
            Assert.That(result, Is.Null);  // Должен вернуть null
        }

        /// <summary>
        /// Тест 12: Поиск с пустой строкой
        /// </summary>
        [Test]
        public void Search_EmptyString_ThrowsArgumentException()
        {
            // ARRANGE
            var service = new ProductService(_configuration);
            
            // ACT & ASSERT
            Assert.That(() => service.Search(""), 
                Throws.ArgumentException);  // Ожидаем ArgumentException
            
            Assert.That(() => service.Search("   "), 
                Throws.ArgumentException);  // Только пробелы - тоже ошибка
        }

        /// <summary>
        /// Тест 13: Получение всех товаров
        /// </summary>
        [Test]
        public void GetAll_ReturnsAllProducts()
        {
            // ARRANGE
            var service = new ProductService(_configuration);
            service.Add(_testProducts[0]);
            service.Add(_testProducts[1]);
            
            // ACT
            var result = service.GetAll();
            
            // ASSERT
            Assert.That(result.Count(), Is.EqualTo(2));  // Должно быть 2 товара
        }

        /// <summary>
        /// Тест 14: Получение товара по существующему ID
        /// </summary>
        [Test]
        public void GetById_ExistingId_ReturnsProduct()
        {
            // ARRANGE
            var service = new ProductService(_configuration);
            service.Add(_testProducts[0]);
            
            // ACT
            var result = service.GetById(_testProducts[0].Id);
            
            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(_testProducts[0].Id));
            Assert.That(result.Name, Is.EqualTo(_testProducts[0].Name));
        }

        /// <summary>
        /// Тест 15: Получение товара по несуществующему ID
        /// </summary>
        [Test]
        public void GetById_NonExistingId_ReturnsNull()
        {
            // ARRANGE
            var service = new ProductService(_configuration);
            var nonExistingId = Guid.NewGuid();
            
            // ACT
            var result = service.GetById(nonExistingId);
            
            // ASSERT
            Assert.That(result, Is.Null);  // Должен вернуть null
        }

        /// <summary>
        /// Тест 16: Загрузка из существующего файла при инициализации
        /// </summary>
        [Test]
        public void InitFromFile_WhenFileExists_LoadsProducts()
        {
            // ARRANGE - создаем файл с тестовыми данными
            var jsonText = JsonSerializer.Serialize(_testProducts, 
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_testFilePath, jsonText);
            
            // ACT - создаем сервис (автоматически вызывает InitFromFile)
            var service = new ProductService(_configuration);
            
            // ASSERT - проверяем, что данные загрузились
            var products = service.GetAll();
            Assert.That(products.Count(), Is.EqualTo(3));  // Должно быть 3 товара
            
            var product = service.GetById(_testProducts[0].Id);
            Assert.That(product, Is.Not.Null);
            Assert.That(product!.Name, Is.EqualTo(_testProducts[0].Name));
        }

        /// <summary>
        /// Тест 17: Инициализация без файла
        /// </summary>
        [Test]
        public void InitFromFile_WhenFileDoesNotExist_StartsWithEmptyDictionary()
        {
            // ARRANGE - файла нет
            
            // ACT
            var service = new ProductService(_configuration);
            
            // ASSERT
            var products = service.GetAll();
            Assert.That(products.Count(), Is.EqualTo(0));  // Список пуст
        }

        /// <summary>
        /// Тест 18: Запись в файл с мьютексом
        /// </summary>
        [Test]
        public void WriteToFile_WithMutex_WritesCorrectly()
        {
            // ARRANGE
            var service = new ProductService(_configuration);
            
            // ACT - добавляем несколько товаров (каждый вызовет WriteToFile)
            service.Add(_testProducts[0]);
            service.Add(_testProducts[1]);
            service.Add(_testProducts[2]);
            
            // ASSERT - читаем файл
            var jsonText = File.ReadAllText(_testFilePath);
            var productsFromFile = JsonSerializer.Deserialize<List<Product>>(jsonText);
            
            // Проверяем, что все 3 товара сохранились
            Assert.That(productsFromFile!.Count, Is.EqualTo(3));
        }
    }
}