using Orders.API.Extensions;

namespace Orders.API.ExceptionHandlers
{
    /// <summary>Escribe la respuesta de error con el contrato de la consigna (sección 3.1) más el correlationId.</summary>
    public static class ErrorResponseWriter
    {
        public static async ValueTask WriteAsync(
            HttpContext context, int status, string type, string title,
            string detail, string errorCode, string errorMessage,
            CancellationToken cancellationToken)
        {
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new
            {
                type,
                title,
                status,
                detail,
                instance = context.Request.Path.Value,
                errorCode,
                errorMessage,
                correlationId = context.GetCorrelationId()
            }, cancellationToken);
        }
    }
}