namespace Products.API.Exceptions
{
    /// <summary>Los datos del request son inválidos (400).</summary>
    public class ValidationException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}