// INotificationRepository es el contrato de persistencia de Notifications:
// el NotificationService solo conoce esta interfaz y no sabe si atrás hay SQLite u otra base.
// Ningún repositorio hace llamadas HTTP: validar al usuario contra Users API es trabajo del Service.
using Notifications.API.Models;

namespace Notifications.API.Data
{
    /// <summary>Operaciones de persistencia de notificaciones.</summary>
    public interface INotificationRepository
    {
        /// <summary>Inserta una notificación nueva.</summary>
        Task Guardar(Notification notification);

        /// <summary>Devuelve las notificaciones de un usuario, de la más reciente a la más antigua. Lista vacía si no tiene.</summary>
        Task<List<Notification>> ObtenerPorUsuario(Guid usuarioId);
    }
}