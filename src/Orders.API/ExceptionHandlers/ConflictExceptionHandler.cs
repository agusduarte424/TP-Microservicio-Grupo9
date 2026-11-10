using Microsoft.AspNetCore.Diagnostics;
using Orders.API.Exceptions;

namespace Orders.API.ExceptionHandlers
{
    /// <summary>Convierte ConflictException en un 409 (ORD-006 transición de estado inválida).</summary>
    public class ConflictExceptionHandler(ILogger<ConflictExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not ConflictException ex) return false;

            logger.LogWarning("Error de negocio {ErrorCode}: {ErrorMessage}", ex.ErrorCode, ex.Message);

            // type, title y detail tal cual el ejemplo de ORD-006 en la consigna
            await ErrorResponseWriter.WriteAsync(
                context, StatusCodes.Status409Conflict,
                "https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict",
                "No se puede modificar el estado.",
                ex.ErrorCode, ex.Message, cancellationToken);
            return true;
        }
    }
}