// Bloque de persistencia: registra el repositorio y crea la base al arrancar.
// Program.cs lo usa con dos líneas: builder.Services.AddPersistence() y app.InitializeDatabase().
using Notifications.API.Data;

namespace Notifications.API.Extensions
{
    public static class PersistenceExtensions
    {
        /// <summary>Registra en DI el inicializador y el repositorio. Los Services piden INotificationRepository y reciben NotificationRepository.</summary>
        public static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            services.AddSingleton<DatabaseInitializer>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
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