namespace Orders.API.Exceptions
{
    /// <summary>Se viola una regla de negocio que impide procesar la orden, como el stock insuficiente (422).</summary>
    public class BusinessRuleException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}