namespace Orders.API.Models
{
    /// <summary>Orden de compra de un usuario.</summary>
    public class Order
    {
        /// <summary>Identificador único, se genera al crear la orden.</summary>
        public Guid Id { get; set; }

        /// <summary>Usuario que hizo la orden (se valida contra Users API).</summary>
        public Guid UsuarioId { get; set; }

        /// <summary>Productos de la orden.</summary>
        public List<OrderItem> Items { get; set; } = [];

        /// <summary>Suma de Cantidad × PrecioUnitario de cada item. Se calcula al crear.</summary>
        public decimal Total { get; set; }

        /// <summary>Pendiente, Confirmada, Enviada, Entregada o Cancelada.</summary>
        public string Estado { get; set; } = EstadoOrden.Pendiente;

        /// <summary>Se asigna al crear (UTC).</summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>Se asigna en cada cambio de estado (UTC). Null si nunca cambió.</summary>
        public DateTime? FechaActualizacion { get; set; }
    }
}