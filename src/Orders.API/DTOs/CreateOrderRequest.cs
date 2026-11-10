using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs
{
    /// <summary>Datos para crear una orden (POST /api/orders). El precio, el total y el estado los pone el servidor.</summary>
    public class CreateOrderRequest
    {
        /// <summary>Usuario que hace la orden (se valida contra Users API).</summary>
        /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
        [Required(ErrorMessage = "El usuarioId es requerido.")]
        public Guid? UsuarioId { get; set; }

        /// <summary>Productos de la orden; al menos uno.</summary>
        [Required(ErrorMessage = "La lista de items es requerida.")]
        public List<CreateOrderItemRequest>? Items { get; set; }
    }
}