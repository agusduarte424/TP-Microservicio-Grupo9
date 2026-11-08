namespace Users.API.Exceptions
{
    /// <summary>El usuario existe pero tiene el acceso bloqueado (403).</summary>
    public class ForbiddenException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}