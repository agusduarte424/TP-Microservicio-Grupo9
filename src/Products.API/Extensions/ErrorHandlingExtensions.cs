using Products.API.ExceptionHandlers;
using Microsoft.AspNetCore.Mvc;
using Products.API.Exceptions;

namespace Products.API.Extensions
{
    public static class ErrorHandlingExtensions
    {
        /// <summary>
        /// Registra los handlers de errores. El orden importa: ASP.NET los prueba en secuencia
        /// y usa el primero que reconoce la excepcion, por eso el generico va ultimo.
        /// </summary>
        public static IServiceCollection AddErrorHandling(this IServiceCollection services)
        {
            services.AddExceptionHandler<NotFoundExceptionHandler>();
            services.AddExceptionHandler<ValidationExceptionHandler>();
            services.AddExceptionHandler<BusinessRuleExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            // 400 de validación: reemplaza el 400 automático de [ApiController], que no trae errorCode.
            // Cubre las Data Annotations de los DTOs y también un JSON malformado.
            services.Configure<ApiBehaviorOptions>(options =>
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
                        ErrorCodes.DatosInvalidos, errorMessage);

                    return new ObjectResult(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        title = "Bad Request",
                        status = StatusCodes.Status400BadRequest,
                        detail = "Los datos enviados no son válidos.",
                        instance = context.HttpContext.Request.Path.Value,
                        errorCode = ErrorCodes.DatosInvalidos,
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