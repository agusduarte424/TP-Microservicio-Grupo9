using Notifications.API.Extensions;
using Notifications.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Logging (F2): Serilog se configura antes que cualquier otra cosa
builder.AddSerilogLogging();

// Correlation ID (F3): se manda en las llamadas HTTP salientes (Notifications llama a Users)
builder.Services.AddCorrelationId();
builder.Services.AddErrorHandling();
builder.Services.AddSwaggerDocs();

builder.Services.AddControllers();
builder.Services.AddPersistence();

// Users: Notifications le pregunta si el destinatario existe (NTF-001)
builder.Services.AddHttpClient<IUsersClient, UsersClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:UsersApi"]
        ?? throw new InvalidOperationException("Falta Services:UsersApi en appsettings.json"));
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddScoped<NotificationService>();

var app = builder.Build();

// Correlation ID (F3): va antes del logging de requests, para que "Inicio del request" ya lleve el id
app.UseCorrelationId();

// Logging (F2): log de inicio y fin de cada request
app.UseRequestLogging();
app.UseExceptionHandler();
app.UseSwaggerDocs();

app.MapControllers();
app.InitializeDatabase();

app.Run();