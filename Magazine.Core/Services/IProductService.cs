using System;
using System.Collections.Generic;
using Magazine.Core.Models;

namespace Magazine.Core.Services
{
    public interface IProductService
    {
        Product Add(Product product); //добавление товара
        Product? Remove(Guid id); //удаление товара. ? может вернуть null
        Product Edit(Product product); //Редактирование товара 
        Product? Search(string searchTerm); //Поиск товара. Может вернуть null
        IEnumerable<Product> GetAll(); //Получение всех товаров (коллекция товаров)
        Product? GetById(Guid id); //Получение товара по ID. Может вернуть null
    }
} 

// Интерфейс (контракт) определяет, какие методы должен иметь сервис 