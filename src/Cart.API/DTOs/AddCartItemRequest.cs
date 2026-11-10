using System.ComponentModel.DataAnnotations;

namespace Cart.API.DTOs
{
    /// <summary>Producto a agregar al carrito (POST /api/cart/{userId}/items).</summary>
    public class AddCartItemRequest
    {
        /// <summary>Id del producto en Products API.</summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        [Required(ErrorMessage = "El productoId es requerido.")]
        public Guid? ProductoId { get; set; }

        /// <summary>Unidades a agregar; mayor a 0 (si no, CRT-004).</summary>
        /// <example>2</example>
        public int Cantidad { get; set; }
    }
}