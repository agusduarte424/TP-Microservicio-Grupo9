using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers
{
    /// <summary>
    /// Atrapa cualquier error inesperado y responde 500 con NTF-004. Nunca expone el stack trace.
    /// También atrapa las fallas al llamar a Users API (caída, timeout o un 5xx).
    /// </summary>
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Error inesperado {ErrorCode}", ErrorCodes.ErrorInterno);

            // En Development se muestra el mensaje real; en Production, uno genérico
            var detail = env.IsDevelopment()
                ? exception.Message
                : "Ocurrió un error inesperado.";

            await ErrorResponseWriter.WriteAsync(
                context, StatusCodes.Status500InternalServerError,
                "https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error",
                detail, ErrorCodes.ErrorInterno,
                "Error interno al procesar la notificación.", cancellationToken);
            return true;
        }
    }
}