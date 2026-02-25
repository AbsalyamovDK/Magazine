using System;

namespace Magazine.Core.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Definition { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }

        // Конструктор по умолчанию
        public Product()
        {
            Id = Guid.NewGuid();
            Name = string.Empty;
            Definition = string.Empty;
            Image = string.Empty;
        }

        // Конструктор с параметрами
        public Product(string name, string definition, decimal price, string image = "")
        {
            Id = Guid.NewGuid();
            Name = name ?? string.Empty;
            Definition = definition ?? string.Empty;
            Price = price;
            Image = image ?? string.Empty;
        }
    }
}