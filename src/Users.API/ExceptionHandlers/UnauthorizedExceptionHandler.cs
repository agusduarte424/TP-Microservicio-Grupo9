using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers
{
    /// <summary>Convierte UnauthorizedException en un 401 (credenciales incorrectas).</summary>
    public class UnauthorizedExceptionHandler(ILogger<UnauthorizedExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not UnauthorizedException ex) return false;

            logger.LogWarning("Error de negocio {ErrorCode}: {ErrorMessage}", ex.ErrorCode, ex.Message);

            await ErrorResponseWriter.WriteAsync(
                context, StatusCodes.Status401Unauthorized,
                "https://tools.ietf.org/html/rfc7235#section-3.1", "Unauthorized",
                "Las credenciales no son válidas.",
                ex.ErrorCode, ex.Message, cancellationToken);
            return true;
        }
    }
}