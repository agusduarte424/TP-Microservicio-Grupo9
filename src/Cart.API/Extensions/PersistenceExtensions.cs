using Cart.API.Data;

namespace Cart.API.Extensions
{
    public static class PersistenceExtensions
    {
        /// <summary>Registra el inicializador y el repositorio de carritos.</summary>
        public static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            services.AddSingleton<DatabaseInitializer>();
            services.AddScoped<ICartRepository, CartRepository>();
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