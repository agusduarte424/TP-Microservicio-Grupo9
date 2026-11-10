using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs
{
    /// <summary>Estado nuevo de la orden (PUT /api/orders/{id}/status).</summary>
    public class UpdateOrderStatusRequest
    {
        /// <summary>Pendiente, Confirmada, Enviada, Entregada o Cancelada.</summary>
        /// <example>Confirmada</example>
        [Required(ErrorMessage = "El estado es requerido.")]
        public string? Estado { get; set; }
    }
}