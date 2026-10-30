// Bloque de Correlation ID (F3): un id por request que viaja entre servicios.
// Program.cs lo usa con dos líneas: builder.Services.AddCorrelationId() y app.UseCorrelationId().
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
        /// Registra lo necesario para mandar el Correlation ID en las llamadas HTTP salientes.
        /// Se aplica a todos los HttpClient del servicio, sin tocar cada registro.
        /// </summary>
        public static IServiceCollection AddCorrelationId(this IServiceCollection services)
        {
            // Permite leer el request actual desde fuera de un controller
            services.AddHttpContextAccessor();
            services.AddTransient<CorrelationIdHandler>();

            // Todo HttpClient creado con AddHttpClient pasa por el handler
            services.ConfigureHttpClientDefaults(http => http.AddHttpMessageHandler<CorrelationIdHandler>());

            return services;
        }

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

                // Guardado en el request: lo leen el handler de salida y F4 (respuestas de error)
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

    /// <summary>
    /// Se mete en cada llamada HTTP saliente y le agrega el X-Correlation-Id del request actual.
    /// </summary>
    public class CorrelationIdHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _accessor;

        public CorrelationIdHandler(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var correlationId = _accessor.HttpContext?.GetCorrelationId();

            if (correlationId is not null && !request.Headers.Contains(CorrelationIdExtensions.Header))
            {
                request.Headers.Add(CorrelationIdExtensions.Header, correlationId);
            }

            // Sigue con la llamada normal
            return base.SendAsync(request, cancellationToken);
        }
    }
}