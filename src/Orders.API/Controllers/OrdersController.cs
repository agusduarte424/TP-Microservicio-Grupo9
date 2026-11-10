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


        /// <summary>Obtiene el detalle de una orden.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ErrorExample(404, ErrorCodes.OrdenNoEncontrada, "Orden no encontrada.")]
        [ErrorExample(500, ErrorCodes.ErrorInterno, "Error interno al procesar la orden.")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _service.ObtenerPorId(id));
        }


        /// <summary>Crea una orden en estado Pendiente. Valida usuario, productos y stock.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ErrorExample(400, ErrorCodes.DatosInvalidos, "La orden debe tener al menos un item.")]
        [ErrorExample(404, ErrorCodes.UsuarioNoEncontrado, "Usuario no encontrado al crear la orden.")]
        [ErrorExample(404, ErrorCodes.ProductoNoEncontrado, "Producto no encontrado al crear la orden.")]
        [ErrorExample(422, ErrorCodes.StockInsuficiente, "Stock insuficiente para uno o más productos.")]
        [ErrorExample(500, ErrorCodes.ErrorInterno, "Error interno al procesar la orden.")]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            var orden = await _service.Crear(request);
            return CreatedAtAction(nameof(GetById), new { id = orden.Id }, orden);
        }

    }
}