using Cart.API.Extensions;
using Cart.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Logging (F2): Serilog se configura antes que cualquier otra cosa
builder.AddSerilogLogging();

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

// Logging (F2): log de inicio y fin de cada request
app.UseRequestLogging();

app.MapControllers();

app.Run();