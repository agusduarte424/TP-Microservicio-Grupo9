namespace Notifications.API.DTOs
{
    /// <summary>Notificación registrada. La usan POST /send y GET /{userId}.</summary>
    public class NotificationResponse
    {
        /// <summary>Identificador único de la notificación.</summary>
        /// <example>11112222-3333-4444-5555-666677778888</example>
        public Guid Id { get; set; }

        /// <summary>Usuario destinatario.</summary>
        /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
        public Guid UsuarioId { get; set; }

        /// <summary>Texto de la notificación.</summary>
        /// <example>Su orden #f1e2d3c4 fue confirmada.</example>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>Canal de envío: Email, Push o SMS.</summary>
        /// <example>Email</example>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>Estado del envío. Como el envío es simulado, queda en Enviada.</summary>
        /// <example>Enviada</example>
        public string Estado { get; set; } = string.Empty;

        /// <summary>Fecha en la que se registró el envío.</summary>
        /// <example>2024-03-10T12:01:00Z</example>
        public DateTime FechaEnvio { get; set; }
    }
}