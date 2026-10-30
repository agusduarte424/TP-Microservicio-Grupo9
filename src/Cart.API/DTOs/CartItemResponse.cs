namespace Cart.API.DTOs
{
    /// <summary>Item del carrito en la respuesta.</summary>
    public class CartItemResponse
    {
        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
    }
}