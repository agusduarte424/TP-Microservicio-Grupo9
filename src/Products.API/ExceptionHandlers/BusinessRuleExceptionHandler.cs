using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers
{
    /// <summary>Convierte BusinessRuleException en un 409 (duplicado o producto con ordenes activas).</summary>
    public class BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException ex) return false;

            logger.LogWarning("Error de negocio {ErrorCode}: {ErrorMessage}", ex.ErrorCode, ex.Message);

            // El texto de "detail" depende del caso, igual que en los ejemplos de la consigna
            var detail = ex.ErrorCode == ErrorCodes.ProductoConOrdenesActivas
                ? "No se puede eliminar el recurso."
                : "Ya existe un recurso con esos datos.";

            await ErrorResponseWriter.WriteAsync(
                context, StatusCodes.Status409Conflict,
                "https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict",
                detail, ex.ErrorCode, ex.Message, cancellationToken);
            return true;
        }
    }
}