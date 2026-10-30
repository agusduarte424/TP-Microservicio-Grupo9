namespace Cart.API.DTOs
{
    /// <summary>Carrito tal como lo devuelve la API.</summary>
    public class CartResponse
    {
        public Guid UsuarioId { get; set; }
        public List<CartItemResponse> Items { get; set; } = [];
        public DateTime FechaActualizacion { get; set; }
    }
}