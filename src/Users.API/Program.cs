using Users.API.Extensions;
using Users.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Logging (F2): Serilog se configura antes que cualquier otra cosa
builder.AddSerilogLogging();

// Correlation ID (F3): Users no llama a otros servicios, pero se deja igual
// para que los 5 servicios tengan la misma estructura
builder.Services.AddCorrelationId();
builder.Services.AddErrorHandling();
builder.Services.AddSwaggerDocs();

builder.Services.AddControllers();
builder.Services.AddPersistence();
builder.Services.AddScoped<UserService>();

var app = builder.Build();
app.InitializeDatabase();

// Correlation ID (F3): va antes del logging de requests, para que "Inicio del request" ya lleve el id
app.UseCorrelationId();

// Logging (F2): log de inicio y fin de cada request
app.UseRequestLogging();
app.UseExceptionHandler();
app.UseSwaggerDocs();

app.MapControllers();

app.Run();

