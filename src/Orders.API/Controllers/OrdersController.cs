using Microsoft.AspNetCore.Mvc;
using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Extensions;
using Orders.API.Services;

namespace Orders.API.Controllers
{
    /// <summary>Órdenes de compra.</summary>
    [ApiController]
    [Route("api/orders")]
    [Produces("application/json")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _service;

        public OrdersController(OrderService service)
        {
            _service = service;
        }

        /// <summary>Lista las órdenes. Filtro opcional: ?usuarioId=</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ErrorExample(500, ErrorCodes.ErrorInterno, "Error interno al procesar la orden.")]
        public async Task<IActionResult> GetAll([FromQuery] Guid? usuarioId)
        {
            return Ok(await _service.ObtenerTodas(usuarioId));
        }
    }
}