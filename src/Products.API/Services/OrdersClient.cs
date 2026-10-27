// Cliente real de Orders.API. La URL base sale de appsettings (Services:OrdersApi).
// Si Orders responde un error o no responde, GetFromJsonAsync lanza una excepción:
// el DELETE se corta y el producto NO se borra (va a salir 500 con PRD-005 cuando esté F4).
using System.Net.Http.Json;

namespace Products.API.Services
{
    public class OrdersClient : IOrdersClient
    {
        private static readonly string[] EstadosActivos = ["Pendiente", "Confirmada"];

        private readonly HttpClient _http;

        public OrdersClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> TieneOrdenesActivas(Guid productoId)
        {
            var ordenes = await _http.GetFromJsonAsync<List<OrderDto>>("api/orders") ?? [];

            return ordenes.Any(o =>
                EstadosActivos.Contains(o.Estado) &&
                o.Items.Any(i => i.ProductoId == productoId));
        }

        // solo los campos de la respuesta de Orders que necesitamos
        private sealed class OrderDto
        {
            public string Estado { get; set; } = string.Empty;
            public List<OrderItemDto> Items { get; set; } = [];
        }

        private sealed class OrderItemDto
        {
            public Guid ProductoId { get; set; }
        }
    }
}