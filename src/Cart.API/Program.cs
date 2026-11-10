using Cart.API.Extensions;
using Cart.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Logging (F2): Serilog se configura antes que cualquier otra cosa
builder.AddSerilogLogging();
builder.Services.AddCorrelationId();

builder.Services.AddControllers();
builder.Services.AddErrorHandling();
builder.Services.AddSwaggerDocs();

// Health checks (F5): base propia y servicios de los que depende
builder.Services.AddServiceHealthChecks(builder.Configuration);

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

app.UseCorrelationId();

// Logging (F2): log de inicio y fin de cada request
app.UseRequestLogging();

// Errores (F4): va después del logging de requests, para que un error de negocio se registre como Warning
app.UseExceptionHandler();

// Swagger (F6): documentación en /swagger
app.UseSwaggerDocs();

app.MapControllers();

// Health checks (F5): /health, /health/ready y /health/live
app.MapServiceHealthChecks();

app.Run();