// CartService tiene la lógica del carrito. Usa ICartRepository para los datos
// e IProductsClient para validar productos y stock. No valida el usuario.
using Cart.API.Models;
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

        /// <summary>
        /// Agrega un producto al carrito. Si el usuario no tiene carrito, lo crea.
        /// Si el producto ya está, suma las cantidades.
        /// </summary>
        public async Task<CartResponse> AgregarItem(Guid userId, AddCartItemRequest request)
        {
            ValidarCantidad(request.Cantidad);
            var productoId = request.ProductoId!.Value;
            var producto = await ObtenerProductoOFallar(productoId);

            var cart = await _repository.ObtenerPorUsuario(userId)
                ?? new CartModel { UsuarioId = userId };

            var item = cart.Items.FirstOrDefault(i => i.ProductoId == productoId);
            var cantidadTotal = (item?.Cantidad ?? 0) + request.Cantidad;

            // CRT-003: se compara el total que quedaría en el carrito contra el stock
            if (cantidadTotal > producto.Stock)
                throw new BusinessRuleException(ErrorCodes.StockInsuficiente,
                    $"Stock insuficiente. Disponible: {producto.Stock}, solicitado: {cantidadTotal}.");

            if (item is null)
                cart.Items.Add(new CartItem { ProductoId = productoId, Cantidad = request.Cantidad });
            else
                item.Cantidad = cantidadTotal;

            cart.FechaActualizacion = DateTime.UtcNow;
            await _repository.Guardar(cart);
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

        // CRT-004: la cantidad tiene que ser mayor a cero
        private static void ValidarCantidad(int cantidad)
        {
            if (cantidad <= 0)
                throw new ValidationException(ErrorCodes.CantidadInvalida,
                    "Cantidad inválida. Debe ser mayor a cero.");
        }

        // CRT-002: el producto tiene que existir en Products API.
        // Si Products falla por otra cosa (caído, 500), la excepción sigue de largo → CRT-005.
        private async Task<ProductoInfo> ObtenerProductoOFallar(Guid productoId)
        {
            return await _productsClient.ObtenerProducto(productoId)
                ?? throw new NotFoundException(ErrorCodes.ProductoNoEncontrado, "Producto no encontrado.");
        }
    }
}