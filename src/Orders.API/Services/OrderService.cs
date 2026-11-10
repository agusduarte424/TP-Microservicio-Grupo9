// OrderService tiene la lógica de las órdenes. Usa IOrderRepository para los datos
// e IUsersClient / IProductsClient para validar usuario, productos y stock al crear.
using Orders.API.Data;
using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Models;

namespace Orders.API.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _repository;
        private readonly IUsersClient _usersClient;
        private readonly IProductsClient _productsClient;

        public OrderService(IOrderRepository repository, IUsersClient usersClient, IProductsClient productsClient)
        {
            _repository = repository;
            _usersClient = usersClient;
            _productsClient = productsClient;
        }

        /// <summary>Lista las órdenes; si viene usuarioId, solo las de ese usuario.</summary>
        public async Task<IEnumerable<OrderResponse>> ObtenerTodas(Guid? usuarioId)
        {
            var ordenes = await _repository.ObtenerTodos(usuarioId);
            return ordenes.Select(ToResponse);
        }

        /// <summary>Devuelve una orden o lanza ORD-001 si no existe.</summary>
        public async Task<OrderResponse> ObtenerPorId(Guid id)
        {
            var order = await BuscarOFallar(id);
            return ToResponse(order);
        }

        /// <summary>
        /// Crea una orden en estado Pendiente. Valida los datos (ORD-002), el usuario (ORD-003),
        /// cada producto (ORD-004) y el stock (ORD-005). El precio se toma de Products en este momento.
        /// </summary>
        public async Task<OrderResponse> Crear(CreateOrderRequest request)
        {
            // 1. ORD-002: datos completos y cantidades válidas
            var items = ValidarDatos(request);
            var usuarioId = request.UsuarioId!.Value;

            // 2. ORD-003: el usuario tiene que existir en Users
            if (!await _usersClient.Existe(usuarioId))
                throw new NotFoundException(ErrorCodes.UsuarioNoEncontrado, "Usuario no encontrado al crear la orden.");

            // 3. ORD-004 y ORD-005: cada producto tiene que existir y tener stock suficiente
            var orderItems = new List<OrderItem>();
            foreach (var item in items)
            {
                var producto = await _productsClient.ObtenerProducto(item.ProductoId)
                    ?? throw new NotFoundException(ErrorCodes.ProductoNoEncontrado,
                        $"Producto no encontrado al crear la orden: {item.ProductoId}.");

                if (item.Cantidad > producto.Stock)
                    throw new BusinessRuleException(ErrorCodes.StockInsuficiente,
                        $"Stock insuficiente para uno o más productos. '{producto.Nombre}': disponible {producto.Stock}, solicitado {item.Cantidad}.");

                // el precio se copia de Products ahora: si después cambia allá, esta orden no se modifica
                orderItems.Add(new OrderItem
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = producto.Precio
                });
            }

            // 4. se arma la orden: Pendiente, con el total calculado y la fecha actual
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                Items = orderItems,
                Total = orderItems.Sum(i => i.Cantidad * i.PrecioUnitario),
                Estado = EstadoOrden.Pendiente,
                FechaCreacion = DateTime.UtcNow
            };

            await _repository.Crear(order);
            return ToResponse(order);
        }

        /// <summary>
        /// Cambia el estado de una orden. ORD-002 si el estado no es uno de los 5,
        /// ORD-001 si la orden no existe, ORD-006 si la transición no está permitida.
        /// </summary>
        public async Task<OrderStatusResponse> CambiarEstado(Guid id, UpdateOrderStatusRequest request)
        {
            var nuevo = EstadoOrden.Normalizar(request.Estado)
                ?? throw new ValidationException(ErrorCodes.DatosInvalidos,
                    $"Estado inválido. Valores posibles: {string.Join(", ", EstadoOrden.Todos)}.");

            var order = await BuscarOFallar(id);

            if (!EstadoOrden.PuedeCambiar(order.Estado, nuevo))
                throw new ConflictException(ErrorCodes.TransicionInvalida,
                    $"Una orden en estado '{order.Estado}' no puede pasar a '{nuevo}'.");

            var fecha = DateTime.UtcNow;
            await _repository.ActualizarEstado(order.Id, nuevo, fecha);

            return new OrderStatusResponse
            {
                Id = order.Id,
                Estado = nuevo,
                FechaActualizacion = fecha
            };
        }

        // ORD-002: junta todos los problemas en un solo mensaje, separados por punto y coma.
        // Si un producto viene repetido, se suman sus cantidades en un solo item.
        private static List<(Guid ProductoId, int Cantidad)> ValidarDatos(CreateOrderRequest request)
        {
            var problemas = new List<string>();

            if (request.UsuarioId is null || request.UsuarioId == Guid.Empty)
                problemas.Add("El usuarioId es requerido.");

            if (request.Items is null || request.Items.Count == 0)
            {
                problemas.Add("La orden debe tener al menos un item.");
            }
            else
            {
                if (request.Items.Any(i => i.ProductoId is null || i.ProductoId == Guid.Empty))
                    problemas.Add("Todos los items necesitan un productoId.");

                if (request.Items.Any(i => i.Cantidad <= 0))
                    problemas.Add("La cantidad de cada item debe ser mayor a cero.");
            }

            if (problemas.Count > 0)
                throw new ValidationException(ErrorCodes.DatosInvalidos, string.Join("; ", problemas));

            return request.Items!
                .GroupBy(i => i.ProductoId!.Value)
                .Select(g => (ProductoId: g.Key, Cantidad: g.Sum(i => i.Cantidad)))
                .ToList();
        }

        // se reutiliza en GET /{id} y en PUT /status
        private async Task<Order> BuscarOFallar(Guid id)
        {
            return await _repository.ObtenerPorId(id)
                ?? throw new NotFoundException(ErrorCodes.OrdenNoEncontrada, "Orden no encontrada.");
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