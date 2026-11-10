namespace Cart.API.DTOs
{
    /// <summary>Nueva cantidad de un item (PUT /api/cart/{userId}/items/{productId}).</summary>
    public class UpdateCartItemRequest
    {
        /// <summary>Cantidad nueva; mayor a 0 (si no, CRT-004).</summary>
        /// <example>4</example>
        public int Cantidad { get; set; }
    }
}