using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Magazine.Core.Models;
using Magazine.Core.Services;
using Magazine.WebApi.Services;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Magazine.Tests
{
    [TestFixture]
    public class TestsProductService
    {
        private string _testDbPath;
        private IConfiguration _configuration;
        private IProductService _service;

        [SetUp]
        public void Setup()
        {
            // Создаем уникальное имя для тестовой БД
            _testDbPath = $"test_products_{Guid.NewGuid()}.db";
            
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Database:FilePath", _testDbPath}
            };
            
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
            
            _service = new ProductService(_configuration);
        }

        [TearDown]
        public void TearDown()
        {
            // Удаляем тестовую БД после тестов
            if (File.Exists(_testDbPath))
            {
                File.Delete(_testDbPath);
            }
        }

        [Test]
        public void Add_ValidProduct_AddsToDatabase()
        {
            // Arrange
            var product = new Product
            {
                Name = "Тестовый товар",
                Definition = "Описание",
                Price = 100.50m,
                Image = "test.jpg"
            };

            // Act
            var result = _service.Add(product);
            var allProducts = _service.GetAll();

            // Assert
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(allProducts.Count(), Is.EqualTo(1));
            Assert.That(allProducts.First().Name, Is.EqualTo("Тестовый товар"));
        }

        [Test]
        public void Add_DuplicateId_ThrowsException()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Товар 1"
            };
            
            _service.Add(product);

            // Act & Assert
            var duplicate = new Product
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Товар 2"
            };
            
            Assert.That(() => _service.Add(duplicate), 
                Throws.InvalidOperationException);
        }

        [Test]
        public void Remove_ExistingId_RemovesFromDatabase()
        {
            // Arrange
            var product = new Product { Name = "Для удаления" };
            _service.Add(product);

            // Act
            var removed = _service.Remove(product.Id);
            var allProducts = _service.GetAll();

            // Assert
            Assert.That(removed, Is.Not.Null);
            Assert.That(removed!.Id, Is.EqualTo(product.Id));
            Assert.That(allProducts.Count(), Is.EqualTo(0));
        }

        [Test]
        public void Edit_ExistingProduct_UpdatesInDatabase()
        {
            // Arrange
            var product = new Product 
            { 
                Name = "Старое название",
                Price = 100m 
            };
            _service.Add(product);

            // Act
            product.Name = "Новое название";
            product.Price = 200m;
            
            var updated = _service.Edit(product);
            var fromDb = _service.GetById(product.Id);

            // Assert
            Assert.That(updated.Name, Is.EqualTo("Новое название"));
            Assert.That(fromDb!.Name, Is.EqualTo("Новое название"));
            Assert.That(fromDb.Price, Is.EqualTo(200m));
        }

        [Test]
        public void Search_ExistingTerm_ReturnsProduct()
        {
            // Arrange
            _service.Add(new Product { Name = "Samsung телефон" });
            _service.Add(new Product { Name = "iPhone" });

            // Act
            var result = _service.Search("Samsung");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Samsung телефон"));
        }

        [Test]
        public void Search_NonExistingTerm_ReturnsNull()
        {
            // Arrange
            _service.Add(new Product { Name = "Samsung" });

            // Act
            var result = _service.Search("Nokia");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetById_ExistingId_ReturnsProduct()
        {
            // Arrange
            var product = new Product { Name = "Тест" };
            _service.Add(product);

            // Act
            var result = _service.GetById(product.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(product.Id));
        }

        [Test]
        public void GetById_NonExistingId_ReturnsNull()
        {
            // Act
            var result = _service.GetById(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Null);
        }
    }
}