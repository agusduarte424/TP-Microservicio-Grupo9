using Microsoft.AspNetCore.Diagnostics;
using Orders.API.Exceptions;

namespace Orders.API.ExceptionHandlers
{
    /// <summary>
    /// Atrapa cualquier error inesperado y responde 500 con ORD-007. Nunca expone el stack trace.
    /// También atrapa las fallas al llamar a Users o Products (caídos, timeout o 5xx).
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
                "Error interno al procesar la orden.", cancellationToken);
            return true;
        }
    }
}