// Atributo para documentar en Swagger un error de un endpoint:
// [ErrorExample(404, ErrorCodes.ProductoNoEncontrado, "Producto no encontrado.")]
// El filtro ErrorExamplesOperationFilter lo convierte en un ejemplo con el contrato de la consigna (sección 3.1).
namespace Products.API.Extensions
{
    /// <summary>Documenta en Swagger un posible error de un endpoint, con su errorCode y errorMessage.</summary>
    /// <param name="statusCode">Status HTTP del error (400, 404, 409, 500...).</param>
    /// <param name="errorCode">Código del catálogo de la consigna, por ejemplo PRD-001.</param>
    /// <param name="errorMessage">Mensaje del catálogo, por ejemplo "Producto no encontrado.".</param>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ErrorExampleAttribute(int statusCode, string errorCode, string errorMessage) : Attribute
    {
        public int StatusCode { get; } = statusCode;
        public string ErrorCode { get; } = errorCode;
        public string ErrorMessage { get; } = errorMessage;

        /// <summary>Texto del campo "detail". Si no se indica, se usa el genérico de ese status.</summary>
        public string? Detail { get; set; }

        /// <summary>Ruta de ejemplo para el campo "instance". Si no se indica, se usa la ruta del endpoint.</summary>
        public string? Instance { get; set; }
    }
}