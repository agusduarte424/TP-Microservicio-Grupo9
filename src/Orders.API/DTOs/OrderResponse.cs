namespace Orders.API.DTOs
{
    /// <summary>Orden tal como la devuelve la API (GET y POST).</summary>
    public class OrderResponse
    {
        /// <summary>Id de la orden.</summary>
        /// <example>f1e2d3c4-0000-0000-0000-aabbccddeeff</example>
        public Guid Id { get; set; }

        /// <summary>Usuario que hizo la orden.</summary>
        /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
        public Guid UsuarioId { get; set; }

        /// <summary>Productos de la orden.</summary>
        public List<OrderItemResponse> Items { get; set; } = [];

        /// <summary>Suma de cantidad × precio unitario de cada item.</summary>
        /// <example>3000.00</example>
        public decimal Total { get; set; }

        /// <summary>Pendiente, Confirmada, Enviada, Entregada o Cancelada.</summary>
        /// <example>Pendiente</example>
        public string Estado { get; set; } = string.Empty;

        /// <summary>Fecha de creación (UTC).</summary>
        /// <example>2024-03-10T11:00:00Z</example>
        public DateTime FechaCreacion { get; set; }
    }
}