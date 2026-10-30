namespace Cart.API.Models
{
    /// <summary>Carrito de un usuario. Hay uno solo por usuario, identificado por su UsuarioId.</summary>
    public class Cart
    {
        /// <summary>Usuario dueño del carrito.</summary>
        public Guid UsuarioId { get; set; }

        /// <summary>Productos en el carrito.</summary>
        public List<CartItem> Items { get; set; } = [];

        /// <summary>Se actualiza en cada operación (UTC).</summary>
        public DateTime FechaActualizacion { get; set; }
    }
}