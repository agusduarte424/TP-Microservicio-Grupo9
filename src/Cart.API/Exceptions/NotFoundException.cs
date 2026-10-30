namespace Cart.API.Exceptions
{
    /// <summary>El recurso pedido no existe (404).</summary>
    public class NotFoundException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}