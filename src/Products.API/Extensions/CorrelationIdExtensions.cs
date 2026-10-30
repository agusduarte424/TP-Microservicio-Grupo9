// Bloque de Correlation ID (F3): un id por request que viaja entre servicios.
// Program.cs lo usa con app.UseCorrelationId() (y en el commit 2, builder.Services.AddCorrelationId()).
using Serilog.Context;

namespace Products.API.Extensions
{
    public static class CorrelationIdExtensions
    {
        /// <summary>Nombre del header, igual en los 5 servicios.</summary>
        public const string Header = "X-Correlation-Id";

        // Clave para guardar el id dentro del request (HttpContext.Items)
        private const string ItemKey = "CorrelationId";

        /// <summary>
        /// Middleware: usa el X-Correlation-Id que llega en el request o genera uno nuevo.
        /// Lo guarda en el request, lo devuelve en la respuesta y lo agrega a todos los logs.
        /// </summary>
        public static WebApplication UseCorrelationId(this WebApplication app)
        {
            app.Use(async (context, next) =>
            {
                // Si otro servicio ya mandó un id, se respeta; si no, se crea uno nuevo
                var recibido = context.Request.Headers[Header].ToString();
                var correlationId = string.IsNullOrWhiteSpace(recibido)
                    ? Guid.NewGuid().ToString()
                    : recibido;

                // Guardado en el request: lo leen el commit 2 (llamadas salientes) y F4 (respuestas de error)
                context.Items[ItemKey] = correlationId;

                // Se agrega al header de la respuesta justo antes de enviarla,
                // así sobrevive aunque el manejador de errores limpie la respuesta
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers[Header] = correlationId;
                    return Task.CompletedTask;
                });

                // Todo log escrito dentro de este using lleva el CorrelationId
                using (LogContext.PushProperty("CorrelationId", correlationId))
                {
                    await next(context);
                }
            });

            return app;
        }

        /// <summary>Devuelve el Correlation ID del request actual, o null si no hay.</summary>
        public static string? GetCorrelationId(this HttpContext context)
        {
            return context.Items.TryGetValue(ItemKey, out var valor) ? valor as string : null;
        }
    }
}