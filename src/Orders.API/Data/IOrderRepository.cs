using Orders.API.Models;

namespace Orders.API.Data
{
    /// <summary>Persistencia de órdenes. El OrderService solo usa esta interfaz.</summary>
    public interface IOrderRepository
    {
        /// <summary>Devuelve todas las órdenes con sus items. Si viene usuarioId, solo las de ese usuario.</summary>
        Task<IEnumerable<Order>> ObtenerTodos(Guid? usuarioId);

        /// <summary>Devuelve la orden con sus items, o null si no existe.</summary>
        Task<Order?> ObtenerPorId(Guid id);
    }
}