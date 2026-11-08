// IUsersClient es el contrato para consultar a Users.API.
// El NotificationService solo conoce esta interfaz, no sabe cómo se hace la llamada HTTP.
namespace Notifications.API.Services
{
    /// <summary>Consulta a Users.API.</summary>
    public interface IUsersClient
    {
        /// <summary>
        /// Indica si el usuario existe. Devuelve false solo si Users responde 404.
        /// Si Users no responde o responde un error, lanza una excepción (termina en NTF-004).
        /// </summary>
        Task<bool> ExisteUsuario(Guid usuarioId);
    }
}