namespace Users.API.Exceptions
{
    /// <summary>Las credenciales no coinciden (401).</summary>
    public class UnauthorizedException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}