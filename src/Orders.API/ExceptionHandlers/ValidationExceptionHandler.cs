using Microsoft.AspNetCore.Diagnostics;
using Orders.API.Exceptions;

namespace Orders.API.ExceptionHandlers
{
    /// <summary>Convierte ValidationException en un 400 (ORD-002 datos inválidos).</summary>
    public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not ValidationException ex) return false;

            logger.LogWarning("Error de negocio {ErrorCode}: {ErrorMessage}", ex.ErrorCode, ex.Message);

            await ErrorResponseWriter.WriteAsync(
                context, StatusCodes.Status400BadRequest,
                "https://tools.ietf.org/html/rfc7231#section-6.5.1", "Bad Request",
                "Los datos enviados no son válidos.",
                ex.ErrorCode, ex.Message, cancellationToken);
            return true;
        }
    }
}