using Orders.API.Extensions;

var builder = WebApplication.CreateBuilder(args);


// Logging (F2): Serilog se configura antes que cualquier otra cosa
builder.AddSerilogLogging();

builder.Services.AddControllers();
builder.Services.AddPersistence();

var app = builder.Build();

app.InitializeDatabase();


// Logging (F2): log de inicio y fin de cada request
app.UseRequestLogging();

app.MapControllers();

app.Run();