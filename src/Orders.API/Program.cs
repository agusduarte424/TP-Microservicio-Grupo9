using Orders.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Logging (F2): Serilog se configura antes que cualquier otra cosa
builder.AddSerilogLogging();

// Correlation ID (F3): lo manda en las llamadas HTTP salientes
builder.Services.AddCorrelationId();

builder.Services.AddControllers();

builder.Services.AddErrorHandling();

builder.Services.AddPersistence();

var app = builder.Build();

app.InitializeDatabase();

// Correlation ID (F3): va antes del logging de requests, para que "Inicio del request" ya lleve el id
app.UseCorrelationId();

// Logging (F2): log de inicio y fin de cada request
app.UseRequestLogging();


// Errores (F4): va después del logging de requests, para que un error de negocio se registre como Warning
app.UseExceptionHandler();

app.MapControllers();

app.Run();