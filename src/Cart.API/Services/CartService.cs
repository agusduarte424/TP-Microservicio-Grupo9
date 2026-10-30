// CartService tiene la lógica del carrito. Usa ICartRepository para los datos
// e IProductsClient para validar productos y stock. No valida el usuario.
using Cart.API.Data;
using Cart.API.DTOs;
using Cart.API.Exceptions;
using CartModel = Cart.API.Models.Cart;

namespace Cart.API.Services
{
    public class CartService
    {
        private readonly ICartRepository _repository;
        private readonly IProductsClient _productsClient;

        public CartService(ICartRepository repository, IProductsClient productsClient)
        {
            _repository = repository;
            _productsClient = productsClient;
        }

        /// <summary>Devuelve el carrito o lanza CRT-001 si el usuario no tiene.</summary>
        public async Task<CartResponse> ObtenerCarrito(Guid userId)
        {
            var cart = await BuscarOFallar(userId);
            return ToResponse(cart);
        }

        // se reutiliza en GET, PUT y los DELETE
        private async Task<CartModel> BuscarOFallar(Guid userId)
        {
            return await _repository.ObtenerPorUsuario(userId)
                ?? throw new NotFoundException(ErrorCodes.CarritoNoEncontrado, "Carrito no encontrado.");
        }

        private static CartResponse ToResponse(CartModel cart) => new()
        {
            UsuarioId = cart.UsuarioId,
            FechaActualizacion = cart.FechaActualizacion,
            Items = cart.Items
                .Select(i => new CartItemResponse { ProductoId = i.ProductoId, Cantidad = i.Cantidad })
                .ToList()
        };
    }
}