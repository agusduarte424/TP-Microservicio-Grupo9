using Cart.API.ExceptionHandlers;
using Cart.API.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Cart.API.Extensions
{
    public static class ErrorHandlingExtensions
    {
        /// <summary>
        /// Registra los handlers de errores. El orden importa: ASP.NET los prueba en secuencia
        /// y usa el primero que reconoce la excepción, por eso el genérico va último.
        /// </summary>
        public static IServiceCollection AddErrorHandling(this IServiceCollection services)
        {
            services.AddExceptionHandler<NotFoundExceptionHandler>();
            services.AddExceptionHandler<ValidationExceptionHandler>();
            services.AddExceptionHandler<BusinessRuleExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            // 400 de validación: reemplaza el 400 automático de [ApiController], que no trae errorCode.
            // Cubre un productoId faltante y también un JSON malformado.
            // PostConfigure corre siempre después de AddControllers(), así que MVC no lo pisa.
            // El único código 400 del catálogo de Cart es CRT-004.
            services.PostConfigure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    // Junta todos los problemas en un solo texto, separados por punto y coma
                    var mensajes = context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                            ? "El cuerpo de la solicitud no es un JSON válido."
                            : e.ErrorMessage)
                        .Distinct();
                    var errorMessage = string.Join("; ", mensajes);

                    // Mismo criterio que los handlers: error de negocio = Warning, con el errorCode en el log
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("ValidacionDeModelo");
                    logger.LogWarning("Error de negocio {ErrorCode}: {ErrorMessage}",
                        ErrorCodes.CantidadInvalida, errorMessage);

                    return new ObjectResult(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        title = "Bad Request",
                        status = StatusCodes.Status400BadRequest,
                        detail = "Los datos enviados no son válidos.",
                        instance = context.HttpContext.Request.Path.Value,
                        errorCode = ErrorCodes.CantidadInvalida,
                        errorMessage,
                        correlationId = context.HttpContext.GetCorrelationId()
                    })
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                };
            });

            return services;
        }
    }
}