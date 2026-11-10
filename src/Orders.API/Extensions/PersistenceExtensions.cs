// Bloque de persistencia: registra lo necesario para la base y la crea al arrancar.
// Program.cs lo usa con dos líneas: builder.Services.AddPersistence() y app.InitializeDatabase().
using Orders.API.Data;

namespace Orders.API.Extensions
{
    public static class PersistenceExtensions
    {
        /// <summary>Registra el inicializador de la base. En el próximo commit se suma el repositorio.</summary>
        public static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            services.AddSingleton<DatabaseInitializer>();
            return services;
        }

        /// <summary>Crea las tablas si no existen. Se llama antes de app.Run().</summary>
        public static WebApplication InitializeDatabase(this WebApplication app)
        {
            app.Services.GetRequiredService<DatabaseInitializer>().Initialize();
            return app;
        }
    }
}