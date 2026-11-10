namespace Orders.API.DTOs
{
    /// <summary>Respuesta de PUT /api/orders/{id}/status, igual al ejemplo de la consigna.</summary>
    public class OrderStatusResponse
    {
        /// <summary>Id de la orden.</summary>
        /// <example>f1e2d3c4-0000-0000-0000-aabbccddeeff</example>
        public Guid Id { get; set; }

        /// <summary>Estado nuevo.</summary>
        /// <example>Confirmada</example>
        public string Estado { get; set; } = string.Empty;

        /// <summary>Fecha del cambio de estado (UTC).</summary>
        /// <example>2024-03-10T12:00:00Z</example>
        public DateTime FechaActualizacion { get; set; }
    }
}