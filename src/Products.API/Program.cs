using Products.API.Extensions;
using Products.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddPersistence();
builder.Services.AddScoped<ProductService>();

var app = builder.Build();

app.InitializeDatabase();

app.MapControllers();

app.Run();