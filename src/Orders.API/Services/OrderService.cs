// OrderService tiene la lógica de las órdenes. Usa IOrderRepository para los datos.
// En los próximos commits se suman los clientes de Users y Products para crear órdenes.
using Orders.API.Data;
using Orders.API.DTOs;
using Orders.API.Models;

namespace Orders.API.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _repository;

        public OrderService(IOrderRepository repository)
        {
            _repository = repository;
        }

        /// <summary>Lista las órdenes; si viene usuarioId, solo las de ese usuario.</summary>
        public async Task<IEnumerable<OrderResponse>> ObtenerTodas(Guid? usuarioId)
        {
            var ordenes = await _repository.ObtenerTodos(usuarioId);
            return ordenes.Select(ToResponse);
        }

        // convierte el modelo interno en el DTO que devuelve la API
        private static OrderResponse ToResponse(Order order) => new()
        {
            Id = order.Id,
            UsuarioId = order.UsuarioId,
            Total = order.Total,
            Estado = order.Estado,
            FechaCreacion = order.FechaCreacion,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario
            }).ToList()
        };
    }
}