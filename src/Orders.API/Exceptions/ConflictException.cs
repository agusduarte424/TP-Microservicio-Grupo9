namespace Orders.API.Exceptions
{
    /// <summary>El pedido choca con el estado actual del recurso, como una transición de estado inválida (409).</summary>
    public class ConflictException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}