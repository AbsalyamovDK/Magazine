using System;
using System.IO;
using System.Linq;
using Magazine.Core.Models;
using Magazine.WebApi.Data;
using NUnit.Framework;

namespace Magazine.Tests
{
    /// <summary>
    /// Модульные тесты для класса Database
    /// </summary>
    [TestFixture]
    public class TestsDatabase
    {
        private string _testDbPath;
        private Database _database;

        [SetUp]
        public void Setup()
        {
            _testDbPath = $"test_db_{Guid.NewGuid()}.db";
            _database = new Database(_testDbPath);
        }

        [TearDown]
        public void TearDown()
        {
            _database.Dispose();
            if (File.Exists(_testDbPath))
            {
                File.Delete(_testDbPath);
            }
        }

        [Test]
        public void Constructor_CreatesDatabaseFile()
        {
            // Assert
            Assert.That(File.Exists(_testDbPath), Is.True);
        }

        [Test]
        public void AddProduct_ValidProduct_AddsToDatabase()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Тестовый товар",
                Definition = "Описание",
                Price = 100.50m,
                Image = "test.jpg"
            };

            // Act
            _database.AddProduct(product);
            var allProducts = _database.GetAllProducts();

            // Assert
            Assert.That(allProducts.Count, Is.EqualTo(1));
            Assert.That(allProducts[0].Id, Is.EqualTo(product.Id));
            Assert.That(allProducts[0].Name, Is.EqualTo("Тестовый товар"));
        }

        [Test]
        public void GetProductById_ExistingId_ReturnsProduct()
        {
            // Arrange
            var product = new Product { Name = "Тест" };
            _database.AddProduct(product);

            // Act
            var result = _database.GetProductById(product.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(product.Id));
        }

        [Test]
        public void GetProductById_NonExistingId_ReturnsNull()
        {
            // Act
            var result = _database.GetProductById(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void UpdateProduct_ExistingProduct_UpdatesFields()
        {
            // Arrange
            var product = new Product 
            { 
                Name = "Старое имя",
                Price = 100m 
            };
            _database.AddProduct(product);

            // Act
            product.Name = "Новое имя";
            product.Price = 200m;
            _database.UpdateProduct(product);
            
            var updated = _database.GetProductById(product.Id);

            // Assert
            Assert.That(updated!.Name, Is.EqualTo("Новое имя"));
            Assert.That(updated.Price, Is.EqualTo(200m));
        }

        [Test]
        public void DeleteProduct_ExistingId_RemovesFromDatabase()
        {
            // Arrange
            var product = new Product { Name = "Для удаления" };
            _database.AddProduct(product);

            // Act
            _database.DeleteProduct(product.Id);
            var allProducts = _database.GetAllProducts();

            // Assert
            Assert.That(allProducts.Count, Is.EqualTo(0));
        }

        [Test]
        public void SearchProduct_ExistingTerm_ReturnsProduct()
        {
            // Arrange
            _database.AddProduct(new Product { Name = "Samsung Galaxy" });
            _database.AddProduct(new Product { Name = "iPhone" });

            // Act
            var result = _database.SearchProduct("Samsung");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Samsung Galaxy"));
        }

        [Test]
        public void SearchProduct_NonExistingTerm_ReturnsNull()
        {
            // Arrange
            _database.AddProduct(new Product { Name = "Samsung" });

            // Act
            var result = _database.SearchProduct("Nokia");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetAllProducts_WhenDatabaseEmpty_ReturnsEmptyList()
        {
            // Act
            var products = _database.GetAllProducts();

            // Assert
            Assert.That(products, Is.Empty);
        }

        [Test]
        public void GetAllProducts_WithMultipleProducts_ReturnsAll()
        {
            // Arrange
            _database.AddProduct(new Product { Name = "Товар 1" });
            _database.AddProduct(new Product { Name = "Товар 2" });
            _database.AddProduct(new Product { Name = "Товар 3" });

            // Act
            var products = _database.GetAllProducts();

            // Assert
            Assert.That(products.Count, Is.EqualTo(3));
            Assert.That(products.Select(p => p.Name), 
                Is.EquivalentTo(new[] { "Товар 1", "Товар 2", "Товар 3" }));
        }
    }
}