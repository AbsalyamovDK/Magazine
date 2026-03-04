using System;

namespace Magazine.Core.Models
{
    public class Product
    {
        public Guid Id { get; set; } //уникальный идентификатор 
        public string Name { get; set; } //название товара
        public string Definition { get; set; } //описание товара
        public decimal Price { get; set; } //цена товара
        public string Image { get; set; } //изображение товара

        // Конструктор по умолчанию
        public Product()
        {
            Id = Guid.NewGuid(); //генерируемый новый GUID
            Name = string.Empty; // {
            Definition = string.Empty; // пустые строки
            Image = string.Empty; // }
        }

        // Конструктор с параметрами
        public Product(string name, string definition, decimal price, string image = "")
        {
            Id = Guid.NewGuid(); //id обязательно должен генерироваться
            Name = name ?? string.Empty; // Если null - пустая строка 
            Definition = definition ?? string.Empty; //
            Price = price; //
            Image = image ?? string.Empty; //
        }
    }
}

// Модель данных (сущность). Представляет товар в системе. Конструторы создают объект с готовым ID и защитой 
// от null