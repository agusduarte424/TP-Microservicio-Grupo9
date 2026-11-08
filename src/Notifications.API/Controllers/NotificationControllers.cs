using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
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
        public async Task<IActionResult> Send([FromBody] SendNotificationRequest request)
        {
            var creada = await _service.Enviar(request);
            return StatusCode(StatusCodes.Status201Created, creada);
        }
    }
}