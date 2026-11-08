using Products.API.ExceptionHandlers;

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
            return services;
        }
    }
}