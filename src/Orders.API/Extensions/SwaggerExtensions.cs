// Bloque de Swagger (F6): documentación de la API en /swagger.
// Program.cs lo usa con dos líneas: builder.Services.AddSwaggerDocs() y app.UseSwaggerDocs().
using Microsoft.OpenApi.Models;
using Orders.API.Extensions;
using System.Reflection;

namespace Orders.API.Extensions
{
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Registra el generador de Swagger. Toma los XML comments de los controladores y modelos
        /// (los genera el compilador en Order.API.xml) y agrupa los endpoints por tag.
        /// </summary>
        public static IServiceCollection AddSwaggerDocs(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Orders API",
                    Version = "v1",
                    Description = "Órdenes de compra del E-Commerce."
                });

                // Los XML comments (/// <summary>) de controladores y modelos aparecen como descripciones
                var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                options.IncludeXmlComments(xml, includeControllerXmlComments: true);

                // Convierte los [ErrorExample] de cada endpoint en ejemplos de error
                options.OperationFilter<ErrorExamplesOperationFilter>();
            });

            return services;
        }

        /// <summary>Publica Swagger UI en /swagger. Está disponible en todos los entornos, como pide la consigna.</summary>
        public static WebApplication UseSwaggerDocs(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders API v1");
                options.RoutePrefix = "swagger";
            });

            return app;
        }
    }
}