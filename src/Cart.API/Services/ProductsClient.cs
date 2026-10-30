// Cliente real de Products.API. La URL base sale de Services:ProductsApi.
// Siempre se mira el status antes de leer la respuesta:
// 404 es "no existe" (CRT-002); cualquier otro error corta con excepción (CRT-005).
using System.Net;
using System.Net.Http.Json;

namespace Cart.API.Services
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