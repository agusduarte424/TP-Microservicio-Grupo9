using Microsoft.AspNetCore.Mvc;
using Products.API.DTOs;
using Products.API.Services;

namespace Products.API.Controllers
{
    /// <summary>Catálogo de productos.</summary>
    [ApiController]
    [Route("api/products")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _service;

        public ProductsController(ProductService service)
        {
            _service = service;
        }

        /// <summary>Lista productos. Filtros opcionales: ?categoria= (exacta) y ?nombre= (parcial).</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] string? categoria, [FromQuery] string? nombre)
        {
            return Ok(await _service.ObtenerTodos(categoria, nombre));
        }

        /// <summary>Obtiene un producto por id.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _service.ObtenerPorId(id));
        }
    }
}