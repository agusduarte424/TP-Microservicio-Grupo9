// Bloque de logging (F2): reemplaza el logging de .NET por Serilog.
// Program.cs lo usa con una línea: builder.AddSerilogLogging();


using System.Diagnostics;
using Serilog;
using Serilog.Context;
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

        /// <summary>
        /// Middleware que registra un log al inicio y otro al fin de cada request.
        /// El de fin incluye status y duración, y su nivel depende del resultado:
        /// 2xx Information, 4xx Warning, 5xx o excepción Error.
        /// </summary>
        public static WebApplication UseRequestLogging(this WebApplication app)
        {
            app.Use(async (context, next) =>
            {
                var endpoint = $"{context.Request.Method} {context.Request.Path}";

                // Todo log escrito dentro de este using lleva el Endpoint (gracias a Enrich.FromLogContext)
                using (LogContext.PushProperty("Endpoint", endpoint))
                {
                    var inicio = Stopwatch.GetTimestamp();
                    app.Logger.LogInformation("Inicio del request");

                    try
                    {
                        // Pasa el request al siguiente paso (al final, el controller)
                        await next(context);
                    }
                    catch (Exception ex)
                    {
                        // Excepción que nadie manejó: siempre es Error
                        var msError = Stopwatch.GetElapsedTime(inicio).TotalMilliseconds;
                        app.Logger.LogError(ex, "Fin del request con excepción no controlada en {DuracionMs:0.0} ms", msError);
                        throw;
                    }

                    var status = context.Response.StatusCode;
                    var ms = Stopwatch.GetElapsedTime(inicio).TotalMilliseconds;

                    var nivel = status >= 500 ? LogLevel.Error
                              : status >= 400 ? LogLevel.Warning
                              : LogLevel.Information;

                    app.Logger.Log(nivel, "Fin del request: {StatusCode} en {DuracionMs:0.0} ms", status, ms);
                }
            });

            return app;
        }
    }
}