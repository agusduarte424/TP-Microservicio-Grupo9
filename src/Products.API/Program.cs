using Products.API.Extensions;
using Products.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Logging (F2): Serilog se configura antes que cualquier otra cosa
builder.AddSerilogLogging();

builder.Services.AddControllers();
builder.Services.AddPersistence();
builder.Services.AddScoped<ProductService>();

// Orders: simulado en desarrollo hasta que exista Orders.API, real en el resto
if (builder.Configuration.GetValue<bool>("OrdersSimulado:Usar"))
{
    builder.Services.AddSingleton<IOrdersClient, FakeOrdersClient>();
}
else
{
    builder.Services.AddHttpClient<IOrdersClient, OrdersClient>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["Services:OrdersApi"]
            ?? throw new InvalidOperationException("Falta Services:OrdersApi en appsettings.json"));
        client.Timeout = TimeSpan.FromSeconds(5);
    });
}

var app = builder.Build();

app.InitializeDatabase();

// Correlation ID (F3): va antes del logging de requests, para que "Inicio del request" ya lleve el id
app.UseCorrelationId();

// Logging (F2): log de inicio y fin de cada request
app.UseRequestLogging();

app.MapControllers();

app.Run();