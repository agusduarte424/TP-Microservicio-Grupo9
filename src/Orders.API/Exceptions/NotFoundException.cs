namespace Orders.API.Exceptions
{
    /// <summary>Algo que se buscó no existe (404): la orden, el usuario o un producto.</summary>
    public class NotFoundException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}