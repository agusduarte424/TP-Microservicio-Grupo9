using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs
{
    /// <summary>Producto a incluir en la orden.</summary>
    public class CreateOrderItemRequest
    {
        /// <summary>Id del producto en Products API.</summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        [Required(ErrorMessage = "El productoId es requerido.")]
        public Guid? ProductoId { get; set; }

        /// <summary>Unidades; mayor a 0 (si no, ORD-002).</summary>
        /// <example>2</example>
        public int Cantidad { get; set; }
    }
}