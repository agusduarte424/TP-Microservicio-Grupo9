namespace Orders.API.DTOs
{
    /// <summary>Producto de una orden en la respuesta.</summary>
    public class OrderItemResponse
    {
        /// <summary>Id del producto en Products API.</summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        public Guid ProductoId { get; set; }

        /// <summary>Unidades del producto.</summary>
        /// <example>2</example>
        public int Cantidad { get; set; }

        /// <summary>Precio del producto al momento de crear la orden.</summary>
        /// <example>1500.00</example>
        public decimal PrecioUnitario { get; set; }
    }
}