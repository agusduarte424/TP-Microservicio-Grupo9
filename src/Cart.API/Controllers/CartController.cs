using Microsoft.AspNetCore.Mvc;
using Cart.API.DTOs;
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
        public async Task<IActionResult> AddItem(Guid userId, [FromBody] AddCartItemRequest request)
        {
            return Ok(await _service.AgregarItem(userId, request));
        }
    }
}