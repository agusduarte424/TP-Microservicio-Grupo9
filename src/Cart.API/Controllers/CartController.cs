using Microsoft.AspNetCore.Mvc;
using Cart.API.DTOs;
using Cart.API.Exceptions;
using Cart.API.Extensions;
using Cart.API.Services;

namespace Cart.API.Controllers
{
    /// <summary>Carrito de compras de cada usuario.</summary>
    [ApiController]
    [Route("api/cart")]
    [Produces("application/json")]
    public class CartController : ControllerBase
    {
        private readonly CartService _service;

        public CartController(CartService service)
        {
            _service = service;
        }

        /// <summary>Obtiene el carrito de un usuario.</summary>
        [HttpGet("{userId:guid}")]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ErrorExample(404, ErrorCodes.CarritoNoEncontrado, "Carrito no encontrado.")]
        [ErrorExample(500, ErrorCodes.ErrorInterno, "Error interno al procesar el carrito.")]
        public async Task<IActionResult> Get(Guid userId)
        {
            return Ok(await _service.ObtenerCarrito(userId));
        }

        /// <summary>Agrega un producto al carrito (si ya está, suma la cantidad).</summary>
        [HttpPost("{userId:guid}/items")]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ErrorExample(400, ErrorCodes.CantidadInvalida, "Cantidad inválida. Debe ser mayor a cero.")]
        [ErrorExample(404, ErrorCodes.ProductoNoEncontrado, "Producto no encontrado.")]
        [ErrorExample(422, ErrorCodes.StockInsuficiente, "Stock insuficiente. Disponible: 1, solicitado: 5.")]
        [ErrorExample(500, ErrorCodes.ErrorInterno, "Error interno al procesar el carrito.")]
        public async Task<IActionResult> AddItem(Guid userId, [FromBody] AddCartItemRequest request)
        {
            return Ok(await _service.AgregarItem(userId, request));
        }

        /// <summary>Cambia la cantidad de un producto del carrito.</summary>
        [HttpPut("{userId:guid}/items/{productId:guid}")]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ErrorExample(400, ErrorCodes.CantidadInvalida, "Cantidad inválida. Debe ser mayor a cero.")]
        [ErrorExample(404, ErrorCodes.CarritoNoEncontrado, "Carrito no encontrado.")]
        [ErrorExample(404, ErrorCodes.ProductoNoEncontrado, "El producto no está en el carrito.")]
        [ErrorExample(422, ErrorCodes.StockInsuficiente, "Stock insuficiente. Disponible: 1, solicitado: 5.")]
        [ErrorExample(500, ErrorCodes.ErrorInterno, "Error interno al procesar el carrito.")]
        public async Task<IActionResult> UpdateItem(Guid userId, Guid productId, [FromBody] UpdateCartItemRequest request)
        {
            return Ok(await _service.ActualizarItem(userId, productId, request));
        }

        /// <summary>Quita un producto del carrito.</summary>
        [HttpDelete("{userId:guid}/items/{productId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ErrorExample(404, ErrorCodes.CarritoNoEncontrado, "Carrito no encontrado.")]
        [ErrorExample(404, ErrorCodes.ProductoNoEncontrado, "El producto no está en el carrito.")]
        [ErrorExample(500, ErrorCodes.ErrorInterno, "Error interno al procesar el carrito.")]
        public async Task<IActionResult> RemoveItem(Guid userId, Guid productId)
        {
            await _service.QuitarItem(userId, productId);
            return NoContent();
        }

        /// <summary>Vacía el carrito completo.</summary>
        [HttpDelete("{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ErrorExample(404, ErrorCodes.CarritoNoEncontrado, "Carrito no encontrado.")]
        [ErrorExample(500, ErrorCodes.ErrorInterno, "Error interno al procesar el carrito.")]
        public async Task<IActionResult> Clear(Guid userId)
        {
            await _service.Vaciar(userId);
            return NoContent();
        }
    }
}