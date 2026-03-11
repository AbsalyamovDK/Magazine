using System;
using System.Collections.Generic;
using System.Linq;
using Magazine.Core.Models;
using Magazine.Core.Services;
using Magazine.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Magazine.Tests
{
    /// <summary>
    /// Класс для тестирования ProductController
    /// </summary>
    [TestFixture]
    public class TestsProductController
    {
        private Mock<IProductService> _mockService;
        private ProductController _controller;
        private List<Product> _testProducts;

        /// <summary>
        /// Инициализация перед каждым тестом
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _mockService = new Mock<IProductService>();
            _controller = new ProductController(_mockService.Object);
            
            _testProducts = new List<Product>
            {
                new Product
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Name = "Mock товар 1",
                    Definition = "Mock описание 1",
                    Price = 111.11m,
                    Image = "mock1.jpg"
                },
                new Product
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    Name = "Mock товар 2",
                    Definition = "Mock описание 2",
                    Price = 222.22m,
                    Image = "mock2.jpg"
                },
                new Product
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    Name = "Samsung Mock",
                    Definition = "Mock Samsung описание",
                    Price = 333.33m,
                    Image = "samsung.jpg"
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            // Очистка после тестов
        }

        // ============= ТЕСТЫ ДЛЯ МЕТОДА GETALL =============

        [Test]
        public void GetAll_WhenCalled_ReturnsAllProducts()
        {
            // Arrange
            _mockService.Setup(s => s.GetAll())
                .Returns(_testProducts);
            
            // Act
            var result = _controller.GetAll();
            
            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            
            var products = okResult.Value as IEnumerable<Product>;
            Assert.That(products, Is.Not.Null);
            Assert.That(products.Count(), Is.EqualTo(3));
            
            _mockService.Verify(s => s.GetAll(), Times.Once);
        }

        [Test]
        public void GetAll_WhenServiceThrows_Returns500()
        {
            // Arrange
            _mockService.Setup(s => s.GetAll())
                .Throws(new Exception("Test error"));
            
            // Act
            var result = _controller.GetAll();
            
            // Assert
            var statusCodeResult = result.Result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null);
            Assert.That(statusCodeResult.StatusCode, Is.EqualTo(500));
        }

        // ============= ТЕСТЫ ДЛЯ МЕТОДА GETBYID =============

        [Test]
        public void GetById_ExistingId_ReturnsProduct()
        {
            // Arrange
            var productId = _testProducts[0].Id;
            _mockService.Setup(s => s.GetById(productId))
                .Returns(_testProducts[0]);
            
            // Act
            var result = _controller.GetById(productId);
            
            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            
            var product = okResult.Value as Product;
            Assert.That(product, Is.Not.Null);
            Assert.That(product.Id, Is.EqualTo(productId));
            
            _mockService.Verify(s => s.GetById(productId), Times.Once);
        }

        [Test]
        public void GetById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            _mockService.Setup(s => s.GetById(nonExistingId))
                .Returns((Product?)null);
            
            // Act
            var result = _controller.GetById(nonExistingId);
            
            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public void GetById_EmptyId_ReturnsBadRequest()
        {
            // Arrange
            _mockService.Setup(s => s.GetById(Guid.Empty))
                .Throws(new ArgumentException("Id не может быть пустым"));
            
            // Act
            var result = _controller.GetById(Guid.Empty);
            
            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
        }

        // ============= ТЕСТЫ ДЛЯ МЕТОДА ADD =============

        [Test]
        public void Add_ValidProduct_ReturnsCreated()
        {
            // Arrange
            var newProduct = new Product
            {
                Name = "Новый товар",
                Definition = "Описание",
                Price = 500m
            };
            
            _mockService.Setup(s => s.Add(It.IsAny<Product>()))
                .Returns((Product p) => p);
            
            // Act
            var result = _controller.Add(newProduct);
            
            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.That(createdResult, Is.Not.Null);
            Assert.That(createdResult.StatusCode, Is.EqualTo(201));
            Assert.That(createdResult.ActionName, Is.EqualTo(nameof(_controller.GetById)));
            
            _mockService.Verify(s => s.Add(It.IsAny<Product>()), Times.Once);
        }

        [Test]
        public void Add_NullProduct_ReturnsBadRequest()
        {
            // Arrange
            _mockService.Setup(s => s.Add(null!))
                .Throws(new ArgumentNullException("product"));
            
            // Act
            var result = _controller.Add(null!);
            
            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void Add_DuplicateId_ReturnsConflict()
        {
            // Arrange
            var existingProduct = _testProducts[0];
            
            _mockService.Setup(s => s.Add(It.IsAny<Product>()))
                .Throws(new InvalidOperationException("Товар с таким Id уже существует"));
            
            // Act
            var result = _controller.Add(existingProduct);
            
            // Assert
            var conflictResult = result.Result as ConflictObjectResult;
            Assert.That(conflictResult, Is.Not.Null);
            Assert.That(conflictResult.StatusCode, Is.EqualTo(409));
        }

        // ============= ТЕСТЫ ДЛЯ МЕТОДА REMOVE =============

        [Test]
        public void Remove_ExistingId_ReturnsRemovedProduct()
        {
            // Arrange
            var productId = _testProducts[0].Id;
            _mockService.Setup(s => s.Remove(productId))
                .Returns(_testProducts[0]);
            
            // Act
            var result = _controller.Remove(productId);
            
            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            
            var product = okResult.Value as Product;
            Assert.That(product, Is.Not.Null);
            Assert.That(product.Id, Is.EqualTo(productId));
        }

        [Test]
        public void Remove_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            _mockService.Setup(s => s.Remove(nonExistingId))
                .Returns((Product?)null);
            
            // Act
            var result = _controller.Remove(nonExistingId);
            
            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
        }

        // ============= ТЕСТЫ ДЛЯ МЕТОДА EDIT =============

        [Test]
        public void Edit_ValidData_ReturnsUpdatedProduct()
        {
            // Arrange
            var productId = _testProducts[0].Id;
            var updatedProduct = new Product
            {
                Id = productId,
                Name = "Обновленное название",
                Definition = "Обновленное описание",
                Price = 999.99m
            };
            
            _mockService.Setup(s => s.Edit(It.IsAny<Product>()))
                .Returns(updatedProduct);
            
            // Act
            var result = _controller.Edit(productId, updatedProduct);
            
            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            
            var product = okResult.Value as Product;
            Assert.That(product, Is.Not.Null);
            Assert.That(product.Name, Is.EqualTo("Обновленное название"));
        }

        [Test]
        public void Edit_MismatchedId_ReturnsBadRequest()
        {
            // Arrange
            var urlId = Guid.NewGuid();
            var productBody = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Тест"
            };
            
            // Act
            var result = _controller.Edit(urlId, productBody);
            
            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
            
            _mockService.Verify(s => s.Edit(It.IsAny<Product>()), Times.Never);
        }

        [Test]
        public void Edit_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, Name = "Тест" };
            
            _mockService.Setup(s => s.Edit(It.IsAny<Product>()))
                .Throws(new KeyNotFoundException("Товар не найден"));
            
            // Act
            var result = _controller.Edit(productId, product);
            
            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
        }

        // ============= ТЕСТЫ ДЛЯ МЕТОДА SEARCH =============

        [Test]
        public void Search_ExistingTerm_ReturnsProduct()
        {
            // Arrange
            _mockService.Setup(s => s.Search("Samsung"))
                .Returns(_testProducts[2]);
            
            // Act
            var result = _controller.Search("Samsung");
            
            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            
            var product = okResult.Value as Product;
            Assert.That(product, Is.Not.Null, "Продукт не должен быть null");
            Assert.That(product.Name, Is.EqualTo("Samsung Mock"));
        }

        [Test]
        public void Search_NonExistingTerm_ReturnsNotFound()
        {
            // Arrange
            _mockService.Setup(s => s.Search("NonExistent"))
                .Returns((Product?)null);
            
            // Act
            var result = _controller.Search("NonExistent");
            
            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public void Search_EmptyString_ReturnsBadRequest()
        {
            // Arrange
            _mockService.Setup(s => s.Search(""))
                .Throws(new ArgumentException("Поисковый запрос не может быть пустым"));
            
            // Act
            var result = _controller.Search("");
            
            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
        }
    }
}