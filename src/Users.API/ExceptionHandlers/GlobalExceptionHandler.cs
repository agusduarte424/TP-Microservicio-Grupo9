using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers
{
    /// <summary>Atrapa cualquier error inesperado y responde 500 con USR-006. Nunca expone el stack trace.</summary>
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
                "Error interno al procesar el usuario.", cancellationToken);
            return true;
        }
    }
}