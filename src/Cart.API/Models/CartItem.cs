namespace Cart.API.Models
{
    /// <summary>Un producto dentro del carrito.</summary>
    public class CartItem
    {
        /// <summary>Producto de Products API.</summary>
        public Guid ProductoId { get; set; }

        /// <summary>Unidades; siempre mayor a 0.</summary>
        public int Cantidad { get; set; }
    }
}