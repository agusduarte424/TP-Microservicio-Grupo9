namespace Cart.API.DTOs
{
    /// <summary>Carrito tal como lo devuelve la API.</summary>
    public class CartResponse
    {
        /// <summary>Usuario dueño del carrito.</summary>
        /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
        public Guid UsuarioId { get; set; }

        /// <summary>Productos que hay en el carrito.</summary>
        public List<CartItemResponse> Items { get; set; } = [];

        /// <summary>Fecha de la última operación sobre el carrito (UTC).</summary>
        /// <example>2024-03-10T10:50:00Z</example>
        public DateTime FechaActualizacion { get; set; }
    }
}