namespace Notifications.API.Models
{
    /// <summary>
    /// Modelo que representa una notificación enviada a un usuario (Apéndice A de la consigna).
    /// </summary>
    public class Notification
    {
        /// <summary>Identificador único de la notificación.</summary>
        public Guid Id { get; set; }

        /// <summary>Usuario destinatario.</summary>
        public Guid UsuarioId { get; set; }

        /// <summary>Texto de la notificación. Máximo 500 caracteres.</summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>Canal de envío: Email, Push o SMS.</summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>Estado del envío: Pendiente, Enviada o Fallida.</summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>Fecha en la que se registró el envío.</summary>
        public DateTime FechaEnvio { get; set; }
    }
}