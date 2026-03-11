using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Magazine.Core.Models;
using Magazine.Core.Services;
using Magazine.WebApi.Controllers;
using Magazine.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Magazine.Tests
{
    /// <summary>
    /// Интеграционные тесты - тестируем взаимодействие реальных классов
    /// Без моков, с реальным сервисом и файловой системой
    /// </summary>
    [TestFixture]
    public class IntegrationTests
    {
        private string _testFilePath;
        private IConfiguration _configuration;
        private IProductService _realService;
        private ProductController _controller;

        [SetUp]
        public void Setup()
        {
            // Создаем временный файл для тестов
            _testFilePath = $"integration_test_{Guid.NewGuid()}.json";
            
            // Настраиваем конфигурацию
            var inMemorySettings = new Dictionary<string, string>
            {
                {"DataBaseFilePath", _testFilePath}
            };
            
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
            
            // Создаем РЕАЛЬНЫЙ сервис (не Mock!)
            _realService = new ProductService(_configuration);
            
            // Создаем контроллер с реальным сервисом
            _controller = new ProductController(_realService);
        }

        [TearDown]
        public void TearDown()
        {
            // Удаляем временный файл
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        /// <summary>
        /// Интеграционный тест: Добавление товара через контроллер
        /// </summary>
        [Test]
        public void AddProduct_ThroughController_ProductIsSavedToFile()
        {
            // ARRANGE
            var newProduct = new Product
            {
                Name = "Интеграционный товар",
                Definition = "Интеграционное описание",
                Price = 123.45m,
                Image = "integration.jpg"
            };
            
            // ACT - вызываем метод контроллера
            var actionResult = _controller.Add(newProduct);
            
            // ASSERT - проверяем результат контроллера
            var createdResult = actionResult.Result as CreatedAtActionResult;
            Assert.That(createdResult, Is.Not.Null);
            Assert.That(createdResult.StatusCode, Is.EqualTo(201));
            
            var returnedProduct = createdResult.Value as Product;
            Assert.That(returnedProduct, Is.Not.Null);
            Assert.That(returnedProduct!.Name, Is.EqualTo("Интеграционный товар"));
            
            // Проверяем, что товар сохранился в файл
            Assert.That(File.Exists(_testFilePath), Is.True);
            
            var jsonText = File.ReadAllText(_testFilePath);
            var productsFromFile = JsonSerializer.Deserialize<List<Product>>(jsonText);
            
            Assert.That(productsFromFile!.Count, Is.EqualTo(1));
            Assert.That(productsFromFile[0].Name, Is.EqualTo("Интеграционный товар"));
        }

        /// <summary>
        /// Интеграционный тест: Полный цикл CRUD
        /// </summary>
        [Test]
        public void FullCrudCycle_AllOperations_WorkCorrectly()
        {
            // 1. СОЗДАНИЕ (CREATE)
            var product = new Product
            {
                Name = "CRUD товар",
                Definition = "CRUD описание",
                Price = 999.99m
            };
            
            var createResult = _controller.Add(product);
            var createdResult = createResult.Result as CreatedAtActionResult;
            var createdProduct = createdResult!.Value as Product;
            var productId = createdProduct!.Id;
            
            // 2. ЧТЕНИЕ (READ) - все товары
            var getAllResult = _controller.GetAll();
            var okResult = getAllResult.Result as OkObjectResult;
            var allProducts = okResult!.Value as IEnumerable<Product>;
            Assert.That(allProducts!.Count(), Is.EqualTo(1));
            
            // 3. ЧТЕНИЕ по ID
            var getByIdResult = _controller.GetById(productId);
            var okGetResult = getByIdResult.Result as OkObjectResult;
            var foundProduct = okGetResult!.Value as Product;
            Assert.That(foundProduct!.Name, Is.EqualTo("CRUD товар"));
            
            // 4. ОБНОВЛЕНИЕ (UPDATE)
            var updatedProduct = new Product
            {
                Id = productId,
                Name = "Обновленный CRUD",
                Definition = "Обновленное описание",
                Price = 1999.99m
            };
            
            var updateResult = _controller.Edit(productId, updatedProduct);
            var okUpdateResult = updateResult.Result as OkObjectResult;
            var updated = okUpdateResult!.Value as Product;
            Assert.That(updated!.Name, Is.EqualTo("Обновленный CRUD"));
            
            // 5. ПОИСК (SEARCH)
            var searchResult = _controller.Search("Обновленный");
            var okSearchResult = searchResult.Result as OkObjectResult;
            var found = okSearchResult!.Value as Product;
            Assert.That(found, Is.Not.Null);
            
            // 6. УДАЛЕНИЕ (DELETE)
            var deleteResult = _controller.Remove(productId);
            var okDeleteResult = deleteResult.Result as OkObjectResult;
            var deleted = okDeleteResult!.Value as Product;
            Assert.That(deleted!.Id, Is.EqualTo(productId));
            
            // 7. ПРОВЕРКА, что удалилось
            var finalGetResult = _controller.GetAll();
            var finalOkResult = finalGetResult.Result as OkObjectResult;
            var finalProducts = finalOkResult!.Value as IEnumerable<Product>;
            Assert.That(finalProducts!.Count(), Is.EqualTo(0));
        }

        /// <summary>
        /// Интеграционный тест: Данные сохраняются после перезапуска
        /// </summary>
        [Test]
        public void DataPersistence_AfterServiceRecreation_DataIsLoaded()
        {
            // 1. Создаем товар через первый экземпляр сервиса
            var product1 = new Product
            {
                Name = "Тест сохранения",
                Price = 100m
            };
            
            _controller.Add(product1);
            
            // 2. Создаем НОВЫЙ экземпляр сервиса (имитация перезапуска)
            var newService = new ProductService(_configuration);
            var newController = new ProductController(newService);
            
            // 3. Проверяем, что данные загрузились из файла
            var result = newController.GetAll();
            var okResult = result.Result as OkObjectResult;
            var products = okResult!.Value as IEnumerable<Product>;

            // ИСПРАВЛЕНО: Безопасная проверка без предупреждений
            Assert.That(products, Is.Not.Null);
            Assert.That(products.Count(), Is.EqualTo(1));

            // Используем First() только после проверки, что список не пуст
            var product = products.First();
            Assert.That(product.Name, Is.EqualTo("Тест сохранения"));
        }
    }
}