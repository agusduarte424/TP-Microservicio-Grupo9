using Cart.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Cart.API.ExceptionHandlers
{
    /// <summary>Convierte BusinessRuleException en un 422 (CRT-003 stock insuficiente).</summary>
    public class BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException ex) return false;

            logger.LogWarning("Error de negocio {ErrorCode}: {ErrorMessage}", ex.ErrorCode, ex.Message);

            await ErrorResponseWriter.WriteAsync(
                context, StatusCodes.Status422UnprocessableEntity,
                "https://tools.ietf.org/html/rfc4918#section-11.2", "Unprocessable Entity",
                "No se puede procesar la solicitud.",
                ex.ErrorCode, ex.Message, cancellationToken);
            return true;
        }
    }
}