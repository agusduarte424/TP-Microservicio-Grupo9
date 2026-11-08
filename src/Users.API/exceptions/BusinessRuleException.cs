namespace Users.API.Exceptions
{
    /// <summary>Se viola una regla de negocio: el email ya está registrado (409).</summary>
    public class BusinessRuleException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}