using System;
using System.IO;
using System.Linq;
using Magazine.Core.Models;
using Magazine.Core.Services;
using Magazine.WebApi.Controllers;
using Magazine.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Magazine.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        private string _testDbPath;
        private IConfiguration _configuration;
        private IProductService _service;
        private ProductController _controller;

        [SetUp]
        public void Setup()
        {
            _testDbPath = $"integration_test_{Guid.NewGuid()}.db";
            
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Database:FilePath", _testDbPath}
            };
            
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
            
            _service = new ProductService(_configuration);
            _controller = new ProductController(_service);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_testDbPath))
            {
                File.Delete(_testDbPath);
            }
        }

        [Test]
        public void FullCrudCycle_AllOperations_WorkCorrectly()
        {
            // 1. CREATE - создаем товар через контроллер
            var newProduct = new Product
            {
                Name = "Интеграционный товар",
                Definition = "Описание",
                Price = 999.99m
            };
            
            var createResult = _controller.Add(newProduct);
            var createdResult = createResult.Result as CreatedAtActionResult;
            var createdProduct = createdResult!.Value as Product;
            var productId = createdProduct!.Id;
            
            // 2. READ - проверяем что товар создался
            var getAllResult = _controller.GetAll();
            var okResult = getAllResult.Result as OkObjectResult;
            var allProducts = okResult!.Value as System.Collections.Generic.IEnumerable<Product>;
            Assert.That(allProducts!.Count(), Is.EqualTo(1));
            
            // 3. READ by ID
            var getByIdResult = _controller.GetById(productId);
            var okGetResult = getByIdResult.Result as OkObjectResult;
            var foundProduct = okGetResult!.Value as Product;
            Assert.That(foundProduct!.Name, Is.EqualTo("Интеграционный товар"));
            
            // 4. UPDATE - обновляем товар
            createdProduct.Name = "Обновленный товар";
            createdProduct.Price = 1999.99m;
            
            var updateResult = _controller.Edit(productId, createdProduct);
            var okUpdateResult = updateResult.Result as OkObjectResult;
            var updated = okUpdateResult!.Value as Product;
            Assert.That(updated!.Name, Is.EqualTo("Обновленный товар"));
            
            // 5. SEARCH - ищем обновленный товар
            var searchResult = _controller.Search("Обновленный");
            var okSearchResult = searchResult.Result as OkObjectResult;
            var found = okSearchResult!.Value as Product;
            Assert.That(found, Is.Not.Null);
            
            // 6. DELETE - удаляем товар
            var deleteResult = _controller.Remove(productId);
            var okDeleteResult = deleteResult.Result as OkObjectResult;
            var deleted = okDeleteResult!.Value as Product;
            Assert.That(deleted!.Id, Is.EqualTo(productId));
            
            // 7. VERIFY - проверяем что удалилось
            var finalGetResult = _controller.GetAll();
            var finalOkResult = finalGetResult.Result as OkObjectResult;
            var finalProducts = finalOkResult!.Value as System.Collections.Generic.IEnumerable<Product>;
            Assert.That(finalProducts!.Count(), Is.EqualTo(0));
        }

        [Test]
        public void DataPersistence_AfterServiceRecreation_DataIsLoaded()
        {
            // 1. Создаем товар через первый экземпляр сервиса
            var product = new Product { Name = "Тест сохранения", Price = 100m };
            _controller.Add(product);
            
            // 2. Создаем НОВЫЙ экземпляр сервиса (имитация перезапуска)
            var newService = new ProductService(_configuration);
            var newController = new ProductController(newService);
            
            // 3. Проверяем, что данные загрузились из БД
            var result = newController.GetAll();
            var okResult = result.Result as OkObjectResult;
            var products = okResult!.Value as System.Collections.Generic.IEnumerable<Product>;
            
            Assert.That(products, Is.Not.Null);
            Assert.That(products.Count(), Is.EqualTo(1));
            Assert.That(products.First().Name, Is.EqualTo("Тест сохранения"));
        }
    }
}