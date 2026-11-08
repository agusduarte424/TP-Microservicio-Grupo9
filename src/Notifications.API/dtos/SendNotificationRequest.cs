using System.ComponentModel.DataAnnotations;

namespace Notifications.API.DTOs
{
    /// <summary>Datos para registrar y simular el envío de una notificación (POST /api/notifications/send).</summary>
    public class SendNotificationRequest
    {
        /// <summary>Usuario destinatario. Requerido: tiene que existir en Users API.</summary>
        // Guid? para que [Required] detecte cuando no viene. Sin el ?, llegaría como 00000000-...
        [Required(ErrorMessage = "El usuarioId es requerido.")]
        public Guid? UsuarioId { get; set; }

        /// <summary>Texto de la notificación. Requerido, máximo 500 caracteres.</summary>
        [Required(ErrorMessage = "El mensaje es requerido.")]
        [StringLength(500, ErrorMessage = "El mensaje no puede superar los 500 caracteres.")]
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>Canal de envío. Valores válidos: Email, Push o SMS.</summary>
        [Required(ErrorMessage = "El tipo es requerido.")]
        [AllowedValues("Email", "Push", "SMS", ErrorMessage = "El tipo no es reconocido. Valores válidos: Email, Push o SMS.")]
        public string Tipo { get; set; } = string.Empty;
    }
}