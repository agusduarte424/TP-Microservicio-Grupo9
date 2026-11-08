using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Extensions;
using Notifications.API.Services;

namespace Notifications.API.Controllers
{
    /// <summary>Registro y consulta de notificaciones.</summary>
    [ApiController]
    [Route("api/notifications")]
    [Produces("application/json")]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _service;

        public NotificationsController(NotificationService service)
        {
            _service = service;
        }

        /// <summary>Registra y simula el envío de una notificación a un usuario que existe en Users API.</summary>
        [HttpPost("send")]
        [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ErrorExample(400, ErrorCodes.DatosInvalidos,
            "El mensaje es requerido.; El tipo no es reconocido. Valores válidos: Email, Push o SMS.")]
        [ErrorExample(404, ErrorCodes.UsuarioNoEncontrado, "El usuario destinatario no fue encontrado.")]
        [ErrorExample(500, ErrorCodes.ErrorInterno, "Error interno al procesar la notificación.")]
        public async Task<IActionResult> Send([FromBody] SendNotificationRequest request)
        {
            var creada = await _service.Enviar(request);
            return StatusCode(StatusCodes.Status201Created, creada);
        }

        /// <summary>Lista las notificaciones de un usuario, de la más reciente a la más antigua.</summary>
        [HttpGet("{userId:guid}")]
        [ProducesResponseType(typeof(List<NotificationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ErrorExample(404, ErrorCodes.SinNotificaciones, "No se encontraron notificaciones para el usuario.")]
        [ErrorExample(500, ErrorCodes.ErrorInterno, "Error interno al procesar la notificación.")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            return Ok(await _service.ListarPorUsuario(userId));
        }
    }
}