using Cart.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddPersistence();

var app = builder.Build();

app.InitializeDatabase();

app.MapControllers();

app.Run();