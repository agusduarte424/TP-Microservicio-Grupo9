namespace Notifications.API.DTOs
{
    /// <summary>Notificación registrada. La usan POST /send y GET /{userId}.</summary>
    public class NotificationResponse
    {
        /// <summary>Identificador único de la notificación.</summary>
        public Guid Id { get; set; }

        /// <summary>Usuario destinatario.</summary>
        public Guid UsuarioId { get; set; }

        /// <summary>Texto de la notificación.</summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>Canal de envío: Email, Push o SMS.</summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>Estado del envío. Como el envío es simulado, queda en Enviada.</summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>Fecha en la que se registró el envío.</summary>
        public DateTime FechaEnvio { get; set; }
    }
}