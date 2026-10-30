using CartModel = Cart.API.Models.Cart;

namespace Cart.API.Data
{
    /// <summary>Persistencia de carritos. El CartService solo usa esta interfaz.</summary>
    public interface ICartRepository
    {
        /// <summary>Devuelve el carrito del usuario con sus items, o null si no tiene.</summary>
        Task<CartModel?> ObtenerPorUsuario(Guid usuarioId);

        /// <summary>Crea o actualiza el carrito y reemplaza todos sus items.</summary>
        Task Guardar(CartModel cart);
    }
}