namespace Cart.API.DTOs
{
    /// <summary>Item del carrito en la respuesta.</summary>
    public class CartItemResponse
    {
        /// <summary>Id del producto en Products API.</summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        public Guid ProductoId { get; set; }

        /// <summary>Unidades del producto en el carrito.</summary>
        /// <example>2</example>
        public int Cantidad { get; set; }
    }
}