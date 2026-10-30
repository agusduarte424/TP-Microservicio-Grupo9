namespace Cart.API.Exceptions
{
    /// <summary>Se viola una regla de negocio, por ejemplo stock insuficiente (422).</summary>
    public class BusinessRuleException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}