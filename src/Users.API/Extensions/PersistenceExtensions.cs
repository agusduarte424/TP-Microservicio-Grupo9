// Bloque de persistencia: registra el repositorio y crea la base al arrancar.
// Program.cs lo usa con dos líneas: builder.Services.AddPersistence() y app.InitializeDatabase().
using Users.API.Data;

namespace Users.API.Extensions
{
    public static class PersistenceExtensions
    {
        /// <summary>Registra en DI el inicializador y el repositorio. Los Services piden IUserRepository y reciben UserRepository.</summary>
        public static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            services.AddSingleton<DatabaseInitializer>();
            services.AddScoped<IUserRepository, UserRepository>();
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