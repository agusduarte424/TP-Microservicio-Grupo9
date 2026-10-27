// Orders simulado para probar el DELETE mientras Orders.API no existe.
// Se activa con OrdersSimulado:Usar = true en appsettings.Development.json.
// Los ids que se pongan en OrdersSimulado:ProductosConOrdenesActivas dan PRD-004.
namespace Products.API.Services
{
    public class FakeOrdersClient : IOrdersClient
    {
        private readonly Guid[] _productosConOrdenes;

        public FakeOrdersClient(IConfiguration configuration)
        {
            _productosConOrdenes = configuration
                .GetSection("OrdersSimulado:ProductosConOrdenesActivas")
                .Get<Guid[]>() ?? [];
        }

        public Task<bool> TieneOrdenesActivas(Guid productoId)
        {
            return Task.FromResult(_productosConOrdenes.Contains(productoId));
        }
    }
}