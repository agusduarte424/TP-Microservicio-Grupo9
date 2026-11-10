// Cliente real de Products.API. La URL base sale de Services:ProductsApi en appsettings.json.
// 404 es "no existe" (ORD-004); cualquier otro error corta con excepción (ORD-007).
using System.Net;
using System.Net.Http.Json;

namespace Orders.API.Services
{
    public class ProductsClient : IProductsClient
    {
        private readonly HttpClient _http;

        public ProductsClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<ProductoInfo?> ObtenerProducto(Guid productoId)
        {
            var response = await _http.GetAsync($"api/products/{productoId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProductoInfo>();
        }
    }
}