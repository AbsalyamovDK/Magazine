using System;
using System.Collections.Generic;
using Magazine.Core.Models;

namespace Magazine.Core.Services
{
    public interface IProductService
    {
        Product Add(Product product);
        Product? Remove(Guid id);
        Product Edit(Product product);
        Product? Search(string searchTerm);
        IEnumerable<Product> GetAll();
        Product? GetById(Guid id);
    }
}