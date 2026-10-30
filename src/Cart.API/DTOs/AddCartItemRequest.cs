using System.ComponentModel.DataAnnotations;

namespace Cart.API.DTOs
{
    /// <summary>Producto a agregar al carrito (POST /api/cart/{userId}/items).</summary>
    public class AddCartItemRequest
    {
        /// <summary>Id del producto en Products API.</summary>
        [Required(ErrorMessage = "El productoId es requerido.")]
        public Guid? ProductoId { get; set; }

        /// <summary>Unidades a agregar; mayor a 0 (si no, CRT-004).</summary>
        public int Cantidad { get; set; }
    }
}