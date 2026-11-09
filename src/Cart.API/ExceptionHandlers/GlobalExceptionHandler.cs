using Cart.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Cart.API.ExceptionHandlers
{
    /// <summary>
    /// Atrapa cualquier error inesperado y responde 500 con CRT-005. Nunca expone el stack trace.
    /// También atrapa las fallas al llamar a Products API (caída, timeout o 5xx).
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
                "Error interno al procesar el carrito.", cancellationToken);
            return true;
        }
    }
}