using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.API.Services;

namespace Users.API.Controllers
{
    /// <summary>Registro y autenticación de usuarios.</summary>
    [ApiController]
    [Route("api/users")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _service;

        public UsersController(UserService service)
        {
            _service = service;
        }

        /// <summary>Registra un usuario nuevo.</summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var creado = await _service.Registrar(request);
            return StatusCode(StatusCodes.Status201Created, creado);
        }

        /// <summary>Autentica a un usuario con email y contraseña. Al 3er intento fallido consecutivo se bloquea la cuenta.</summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            return Ok(await _service.Login(request));
        }

        /// <summary>
        /// Endpoint interno: lo usan Orders y Notifications para verificar que un usuario existe.
        /// No está definido en la consigna. Si el usuario no existe responde 404 sin errorCode,
        /// porque el catálogo de Users no tiene un código de "no encontrado".
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var usuario = await _service.ObtenerPorId(id);
            return usuario is null ? NotFound() : Ok(usuario);
        }
    }
}