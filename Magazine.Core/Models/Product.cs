using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Magazine.Core.Models
{
    /// <summary>
    /// Класс товара с атрибутами для Entity Framework
    /// </summary>
    public class Product
    {
        // [Key] - указывает, что это первичный ключ
        [Key]
        // [DatabaseGenerated] - ID генерируется в коде, а не в БД
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        // [Required] - поле обязательно для заполнения
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        // [MaxLength(1000)] - максимальная длина строки
        [MaxLength(1000)]
        public string Definition { get; set; }

        // [Column(TypeName = "decimal(18,2)")] - точность для денег
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [MaxLength(500)]
        public string Image { get; set; }

        // НОВОЕ ПОЛЕ! Дата создания товара
        [Required]
        public DateTime CreatedAt { get; set; }

        // Конструктор по умолчанию
        public Product()
        {
            Id = Guid.NewGuid();
            Name = string.Empty;
            Definition = string.Empty;
            Image = string.Empty;
            CreatedAt = DateTime.UtcNow;
        }

        // Конструктор с параметрами
        public Product(string name, string definition, decimal price, string image = "")
        {
            Id = Guid.NewGuid();
            Name = name ?? string.Empty;
            Definition = definition ?? string.Empty;
            Price = price;
            Image = image ?? string.Empty;
            CreatedAt = DateTime.UtcNow;
        }
    }
}