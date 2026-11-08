// NotificationService tiene la lógica de negocio de Notifications.
// Usa IUsersClient para validar al destinatario e INotificationRepository para guardar,
// y lanza las excepciones del catálogo (NTF-001 a NTF-004).
using Notifications.API.Data;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;

namespace Notifications.API.Services
{
    public class NotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IUsersClient _usersClient;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository repository,
            IUsersClient usersClient,
            ILogger<NotificationService> logger)
        {
            _repository = repository;
            _usersClient = usersClient;
            _logger = logger;
        }

        /// <summary>
        /// Registra y simula el envío de una notificación.
        /// NTF-002 si el usuarioId viene vacío, NTF-001 si el usuario no existe en Users API.
        /// Si Users no responde, la excepción corta el método antes de guardar (NTF-004).
        /// </summary>
        public async Task<NotificationResponse> Enviar(SendNotificationRequest request)
        {
            // [Required] solo detecta que falte; un Guid vacío (00000000-...) llega como valor y se chequea acá
            var usuarioId = request.UsuarioId!.Value;
            if (usuarioId == Guid.Empty)
                throw new ValidationException(ErrorCodes.DatosInvalidos, "El usuarioId no es válido.");

            // se valida al destinatario ANTES de guardar: no se registran notificaciones de usuarios que no existen
            if (!await _usersClient.ExisteUsuario(usuarioId))
                throw new NotFoundException(ErrorCodes.UsuarioNoEncontrado,
                    "El usuario destinatario no fue encontrado.");

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                Mensaje = request.Mensaje.Trim(),
                Tipo = request.Tipo,
                // el envío es simulado: se considera enviada apenas se registra
                Estado = "Enviada",
                FechaEnvio = DateTime.UtcNow
            };

            await _repository.Guardar(notification);

            _logger.LogInformation("Notificación {Tipo} simulada para el usuario {UsuarioId}",
                notification.Tipo, notification.UsuarioId);

            return ToResponse(notification);
        }

        private static NotificationResponse ToResponse(Notification n) => new()
        {
            Id = n.Id,
            UsuarioId = n.UsuarioId,
            Mensaje = n.Mensaje,
            Tipo = n.Tipo,
            Estado = n.Estado,
            FechaEnvio = n.FechaEnvio
        };
    }
}