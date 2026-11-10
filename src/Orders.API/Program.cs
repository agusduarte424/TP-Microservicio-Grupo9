using Orders.API.Extensions;

using Orders.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Logging (F2): Serilog se configura antes que cualquier otra cosa
builder.AddSerilogLogging();

// Correlation ID (F3): lo manda en las llamadas HTTP salientes
builder.Services.AddCorrelationId();

builder.Services.AddControllers();
builder.Services.AddErrorHandling();
builder.Services.AddSwaggerDocs();

// Health checks (F5): base propia y servicios de los que depende
builder.Services.AddServiceHealthChecks(builder.Configuration);

builder.Services.AddPersistence();

builder.Services.AddScoped<OrderService>();


// Users API: valida que el usuario de la orden exista (ORD-003)
builder.Services.AddHttpClient<IUsersClient, UsersClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:UsersApi"]
        ?? throw new InvalidOperationException("Falta Services:UsersApi en appsettings.json"));
    client.Timeout = TimeSpan.FromSeconds(5);
});


// Products API: valida que cada producto exista, su stock y su precio (ORD-004, ORD-005)
builder.Services.AddHttpClient<IProductsClient, ProductsClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProductsApi"]
        ?? throw new InvalidOperationException("Falta Services:ProductsApi en appsettings.json"));
    client.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

app.InitializeDatabase();

// Correlation ID (F3): va antes del logging de requests, para que "Inicio del request" ya lleve el id
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