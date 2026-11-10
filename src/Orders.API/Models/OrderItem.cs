namespace Orders.API.Models
{
    /// <summary>Un producto dentro de una orden.</summary>
    public class OrderItem
    {
        /// <summary>Producto de Products API.</summary>
        public Guid ProductoId { get; set; }

        /// <summary>Unidades; siempre mayor a 0.</summary>
        public int Cantidad { get; set; }

        /// <summary>Precio del producto al momento de crear la orden. No cambia si después cambia en Products.</summary>
        public decimal PrecioUnitario { get; set; }
    }
}