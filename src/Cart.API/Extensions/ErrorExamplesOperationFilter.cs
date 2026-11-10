// Filtro de Swagger: por cada [ErrorExample] de un endpoint agrega un ejemplo en la respuesta
// de ese status, con los mismos 8 campos que devuelven los handlers (sección 3.1 de la consigna
// más el correlationId). Si un status tiene varios errores, los muestra todos como ejemplos
// distintos, uno por errorCode.
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Cart.API.Extensions
{
    public class ErrorExamplesOperationFilter : IOperationFilter
    {
        // type, title y detail genéricos de cada status, tal cual los ejemplos de la consigna
        private static readonly Dictionary<int, (string Type, string Title, string Detail)> PorStatus = new()
        {
            [400] = ("https://tools.ietf.org/html/rfc7231#section-6.5.1", "Bad Request", "Los datos enviados no son válidos."),
            [401] = ("https://tools.ietf.org/html/rfc7235#section-3.1", "Unauthorized", "Las credenciales no son válidas."),
            [403] = ("https://tools.ietf.org/html/rfc7231#section-6.5.3", "Forbidden", "El acceso está prohibido."),
            [404] = ("https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found", "El recurso solicitado no fue encontrado."),
            [409] = ("https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict", "Ya existe un recurso con esos datos."),
            [422] = ("https://tools.ietf.org/html/rfc4918#section-11.2", "Unprocessable Entity", "No se puede procesar la solicitud."),
            [500] = ("https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error", "Ocurrió un error inesperado.")
        };

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var errores = context.MethodInfo.GetCustomAttributes<ErrorExampleAttribute>().ToList();
            if (errores.Count == 0) return;

            // ruta del endpoint como "instance" de ejemplo, por ejemplo /api/cart/{userId}
            var rutaDelEndpoint = "/" + context.ApiDescription.RelativePath;

            foreach (var grupo in errores.GroupBy(e => e.StatusCode))
            {
                var status = grupo.Key;
                var porDefecto = PorStatus.GetValueOrDefault(status,
                    ("https://tools.ietf.org/html/rfc7231", "Error", "Error."));

                // si el endpoint ya declaraba ese status con [ProducesResponseType], se reutiliza
                var clave = status.ToString();
                if (!operation.Responses.TryGetValue(clave, out var respuesta))
                {
                    respuesta = new OpenApiResponse();
                    operation.Responses[clave] = respuesta;
                }
                respuesta.Description = porDefecto.Item2;

                var ejemplos = new Dictionary<string, OpenApiExample>();
                foreach (var error in grupo)
                {
                    ejemplos[error.ErrorCode] = new OpenApiExample
                    {
                        Summary = $"{error.ErrorCode}: {error.ErrorMessage}",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString(porDefecto.Item1),
                            ["title"] = new OpenApiString(porDefecto.Item2),
                            ["status"] = new OpenApiInteger(status),
                            ["detail"] = new OpenApiString(error.Detail ?? porDefecto.Item3),
                            ["instance"] = new OpenApiString(error.Instance ?? rutaDelEndpoint),
                            ["errorCode"] = new OpenApiString(error.ErrorCode),
                            ["errorMessage"] = new OpenApiString(error.ErrorMessage),
                            ["correlationId"] = new OpenApiString("0f8fad5b-d9cb-469f-a165-70867728950e")
                        }
                    };
                }

                // se reemplaza el contenido para que muestre el contrato de la consigna
                // y no el ProblemDetails genérico que .NET agrega solo
                respuesta.Content.Clear();
                respuesta.Content["application/json"] = new OpenApiMediaType
                {
                    Schema = EsquemaDeError(),
                    Examples = ejemplos
                };
            }
        }

        // los 8 campos que devuelven los handlers
        private static OpenApiSchema EsquemaDeError() => new()
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = new() { Type = "string" },
                ["title"] = new() { Type = "string" },
                ["status"] = new() { Type = "integer", Format = "int32" },
                ["detail"] = new() { Type = "string" },
                ["instance"] = new() { Type = "string" },
                ["errorCode"] = new() { Type = "string" },
                ["errorMessage"] = new() { Type = "string" },
                ["correlationId"] = new() { Type = "string" }
            }
        };
    }
}