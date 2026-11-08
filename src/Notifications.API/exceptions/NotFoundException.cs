namespace Notifications.API.Exceptions
{
    /// <summary>Un recurso no existe: el usuario destinatario o sus notificaciones (404).</summary>
    public class NotFoundException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}