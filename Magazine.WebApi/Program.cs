using Magazine.Core.Services;
using Magazine.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IProductService, ProductService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Теперь работает после установки пакета

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();      // Теперь работает
    app.UseSwaggerUI();    // Теперь работает
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();