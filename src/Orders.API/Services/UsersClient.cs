// Cliente real de Users.API. La URL base sale de Services:UsersApi en appsettings.json.
// Siempre se mira el status antes de seguir:
// 404 es "no existe" (ORD-003); cualquier otro error corta con excepción (ORD-007).
using System.Net;

namespace Orders.API.Services
{
    public class UsersClient : IUsersClient
    {
        private readonly HttpClient _http;

        public UsersClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> Existe(Guid usuarioId)
        {
            var response = await _http.GetAsync($"api/users/{usuarioId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return false;

            // 500, 503, etc.: lanza HttpRequestException y el handler global responde ORD-007
            response.EnsureSuccessStatusCode();
            return true;
        }
    }
}