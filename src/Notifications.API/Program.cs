using Notifications.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Logging (F2): Serilog se configura antes que cualquier otra cosa
builder.AddSerilogLogging();

// Correlation ID (F3): se manda en las llamadas HTTP salientes (Notifications llama a Users)
builder.Services.AddCorrelationId();

builder.Services.AddControllers();

var app = builder.Build();

// Correlation ID (F3): va antes del logging de requests, para que "Inicio del request" ya lleve el id
app.UseCorrelationId();

// Logging (F2): log de inicio y fin de cada request
app.UseRequestLogging();

app.MapControllers();

app.Run();