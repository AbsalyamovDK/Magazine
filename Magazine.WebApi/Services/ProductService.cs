using System;
using System.Collections.Generic;
using System.Linq;
using Magazine.Core.Models;
using Magazine.Core.Services;

namespace Magazine.WebApi.Services
{
    public class ProductService : IProductService
    {
        private static readonly List<Product> _products = new List<Product>();

        public Product Add(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (_products.Any(p => p.Id == product.Id))
                throw new InvalidOperationException($"Товар с Id {product.Id} уже существует");

            if (product.Id == Guid.Empty)
                product.Id = Guid.NewGuid();

            _products.Add(product);
            return product;
        }

        public Product? Remove(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id не может быть пустым");

            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product != null)
                _products.Remove(product);
            
            return product;
        }

        public Product Edit(Product updatedProduct)
        {
            if (updatedProduct == null)
                throw new ArgumentNullException(nameof(updatedProduct));

            var existingProduct = _products.FirstOrDefault(p => p.Id == updatedProduct.Id);
            if (existingProduct == null)
                throw new KeyNotFoundException($"Товар с Id {updatedProduct.Id} не найден");

            existingProduct.Name = updatedProduct.Name;
            existingProduct.Definition = updatedProduct.Definition;
            existingProduct.Price = updatedProduct.Price;
            existingProduct.Image = updatedProduct.Image;

            return existingProduct;
        }

        public Product? Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Поисковый запрос не может быть пустым");

            var normalizedSearchTerm = searchTerm.ToLower().Trim();
            
            return _products.FirstOrDefault(p => 
                (p.Name?.ToLower().Contains(normalizedSearchTerm) ?? false) ||
                (p.Definition?.ToLower().Contains(normalizedSearchTerm) ?? false)
            );
        }

        public IEnumerable<Product> GetAll()
        {
            return _products.ToList();
        }

        public Product? GetById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id не может быть пустым");

            return _products.FirstOrDefault(p => p.Id == id);
        }
    }
}