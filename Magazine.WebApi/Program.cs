using Magazine.Core.Services;
using Magazine.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Внедрение зависимостей - связываем интерфейс с реализацией
// Используем Singleton, чтобы данные сохранялись между запросами
builder.Services.AddSingleton<IProductService, ProductService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();