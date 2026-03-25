using Microsoft.EntityFrameworkCore;
using Magazine.Core.Services;
using Magazine.WebApi.Data;
using Magazine.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Получаем строку подключения из конфигурации
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Регистрируем DbContext с SQLite
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlite(connectionString));

// Регистрируем сервис - теперь используем DataBaseProductService
builder.Services.AddScoped<IProductService, DataBaseProductService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Создаём базу данных, если её нет
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    // EnsureCreated создаёт БД, если её нет
    // Не удаляет существующую!
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();