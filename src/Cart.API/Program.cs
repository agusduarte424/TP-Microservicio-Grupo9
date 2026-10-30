using Cart.API.Extensions;
using Cart.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddPersistence();
builder.Services.AddScoped<CartService>();

// Products API: la URL viene de appsettings, nada fijo en el código
builder.Services.AddHttpClient<IProductsClient, ProductsClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProductsApi"]
        ?? throw new InvalidOperationException("Falta Services:ProductsApi en appsettings.json"));
    client.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

app.InitializeDatabase();

app.MapControllers();

app.Run();