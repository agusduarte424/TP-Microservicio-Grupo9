using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers
{
    /// <summary>Convierte BusinessRuleException en un 409 (email ya registrado).</summary>
    public class BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException ex) return false;

            logger.LogWarning("Error de negocio {ErrorCode}: {ErrorMessage}", ex.ErrorCode, ex.Message);

            await ErrorResponseWriter.WriteAsync(
                context, StatusCodes.Status409Conflict,
                "https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict",
                "Ya existe un recurso con esos datos.",
                ex.ErrorCode, ex.Message, cancellationToken);
            return true;
        }
    }
}