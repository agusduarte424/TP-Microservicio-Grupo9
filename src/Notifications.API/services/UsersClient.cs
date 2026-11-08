// Cliente real de Users.API. La URL base sale de appsettings (Services:UsersApi).
// Usa el endpoint interno GET /api/users/{id}.
// Regla del plan: un 404 es un error de negocio de este servicio (NTF-001);
// cualquier otro fallo (timeout, caída, 5xx) lanza una excepción y sale 500 con NTF-004.
using System.Net;

namespace Notifications.API.Services
{
    public class UsersClient : IUsersClient
    {
        private readonly HttpClient _http;

        public UsersClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> ExisteUsuario(Guid usuarioId)
        {
            using var response = await _http.GetAsync($"api/users/{usuarioId}");

            // 404: el usuario no existe. Es el único caso que se trata como "no encontrado".
            if (response.StatusCode == HttpStatusCode.NotFound)
                return false;

            // Se verifica el status ANTES de procesar la respuesta: cualquier otro código que no sea 2xx
            // lanza HttpRequestException, que nadie atrapa y llega al GlobalExceptionHandler (500, NTF-004).
            response.EnsureSuccessStatusCode();

            return true;
        }
    }
}