// Bloque de logging (F2): reemplaza el logging de .NET por Serilog.
// Program.cs lo usa con una línea: builder.AddSerilogLogging();
using Serilog;
using Serilog.Events;
using Serilog.Templates;

namespace Products.API.Extensions
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Configura Serilog con dos salidas: consola (para leer) y archivo JSON (para procesar).
        /// Cada log lleva Servicio, y también Endpoint y CorrelationId cuando existen.
        /// </summary>
        public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
        {
            // Nombre del proyecto ("Products.API"). Así este archivo sirve igual en los 5 servicios.
            var servicio = builder.Environment.ApplicationName;

            // Archivo en src/Products.API/logs/ (la carpeta logs/ está en el .gitignore)
            var rutaArchivo = Path.Combine(builder.Environment.ContentRootPath, "logs", $"{servicio}-.json");

            builder.Host.UseSerilog((context, config) => config
                // Nivel mínimo: Information. Los mensajes internos de Microsoft, solo Warning o peor...
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                // ...salvo los de arranque ("Now listening on..."), que sí queremos ver
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)

                // Suma a cada log lo guardado en el contexto del request (Endpoint, CorrelationId)
                .Enrich.FromLogContext()
                // Suma a cada log el nombre del servicio
                .Enrich.WithProperty("Servicio", servicio)

                // Salida 1: consola legible
                .WriteTo.Console(new ExpressionTemplate(
                    "[{@t:HH:mm:ss} {@l:u3}] {Servicio} {CorrelationId} {Endpoint} - {@m}\n{@x}"))

                // Salida 2: archivo JSON, un objeto por línea, un archivo por día
                .WriteTo.File(
                    new ExpressionTemplate(
                        "{ {Timestamp: @t, Nivel: @l, Servicio, Endpoint, CorrelationId, ErrorCode, Mensaje: @m, Excepcion: @x} }\n"),
                    rutaArchivo,
                    rollingInterval: RollingInterval.Day));

            return builder;
        }
    }
}