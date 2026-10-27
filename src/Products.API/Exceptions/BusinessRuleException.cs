namespace Products.API.Exceptions
{
    /// <summary>Se viola una regla de negocio, por ejemplo un duplicado o un producto con órdenes activas (409).</summary>
    public class BusinessRuleException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}