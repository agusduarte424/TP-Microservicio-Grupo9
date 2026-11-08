using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers
{
    /// <summary>Convierte ForbiddenException en un 403 (usuario bloqueado por intentos o por fraude).</summary>
    public class ForbiddenExceptionHandler(ILogger<ForbiddenExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not ForbiddenException ex) return false;

            logger.LogWarning("Error de negocio {ErrorCode}: {ErrorMessage}", ex.ErrorCode, ex.Message);

            await ErrorResponseWriter.WriteAsync(
                context, StatusCodes.Status403Forbidden,
                "https://tools.ietf.org/html/rfc7231#section-6.5.3", "Forbidden",
                "El acceso está prohibido.",
                ex.ErrorCode, ex.Message, cancellationToken);
            return true;
        }
    }
}