using Microsoft.EntityFrameworkCore;
using Magazine.Core.Models;

namespace Magazine.WebApi.Data
{
    /// <summary>
    /// Контекст базы данных Entity Framework
    /// </summary>
    public class ApplicationContext : DbContext
    {
        /// <summary>
        /// Конструктор - принимает параметры подключения
        /// </summary>
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Таблица Products в базе данных
        /// </summary>
        public DbSet<Product> Products { get; set; }

        /// <summary>
        /// Настройка модели при создании
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка индекса для поля Id
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Id)
                .HasDatabaseName("IX_Products_Id");

            // Настройка индекса для CreatedAt (для быстрой сортировки)
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.CreatedAt)
                .HasDatabaseName("IX_Products_CreatedAt");

            // Настройка индекса для Name (для поиска)
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name)
                .HasDatabaseName("IX_Products_Name");
        }
    }
}