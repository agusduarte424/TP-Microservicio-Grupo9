// NotificationRepository implementa INotificationRepository con SQLite y Dapper:
// el NotificationService le pide que guarde o busque notificaciones
// y el Repository se encarga de hablar con la base notifications.db.
using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Notifications.API.Models;

namespace Notifications.API.Data
{
    /// <summary>Persistencia de notificaciones en SQLite usando Dapper.</summary>
    public class NotificationRepository : INotificationRepository
    {
        private readonly string _connectionString;

        // la cadena de conexión viene de appsettings.json → ConnectionStrings:DefaultConnection
        // si falta, se corta al arrancar en vez de crear una base en un lugar inesperado
        public NotificationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Falta ConnectionStrings:DefaultConnection en appsettings.json");
        }

        // una conexión nueva por operación; SQLite es un archivo local, abrirla es barato
        private SqliteConnection CreateConnection() => new(_connectionString);

        public async Task Guardar(Notification notification)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync("""
                INSERT INTO notifications (id, usuarioId, mensaje, tipo, estado, fechaEnvio)
                VALUES (@Id, @UsuarioId, @Mensaje, @Tipo, @Estado, @FechaEnvio)
            """, new
            {
                Id = notification.Id.ToString(),
                UsuarioId = notification.UsuarioId.ToString(),
                notification.Mensaje,
                notification.Tipo,
                notification.Estado,
                FechaEnvio = notification.FechaEnvio.ToString("o", CultureInfo.InvariantCulture)
            });
        }

        public async Task<List<Notification>> ObtenerPorUsuario(Guid usuarioId)
        {
            using var conn = CreateConnection();

            // la fecha se guarda en formato ISO, así que ordenarla como texto la ordena cronológicamente
            var rows = await conn.QueryAsync<NotificationRow>(
                "SELECT * FROM notifications WHERE usuarioId = @UsuarioId ORDER BY fechaEnvio DESC",
                new { UsuarioId = usuarioId.ToString() });

            return rows.Select(ToNotification).ToList();
        }

        // SQLite guarda el Guid y la fecha como texto:
        // Dapper llena esta fila con los tipos de la base y ToNotification la pasa al modelo
        private sealed class NotificationRow
        {
            public string Id { get; set; } = string.Empty;
            public string UsuarioId { get; set; } = string.Empty;
            public string Mensaje { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string Estado { get; set; } = string.Empty;
            public string FechaEnvio { get; set; } = string.Empty;
        }

        private static Notification ToNotification(NotificationRow row) => new()
        {
            Id = Guid.Parse(row.Id),
            UsuarioId = Guid.Parse(row.UsuarioId),
            Mensaje = row.Mensaje,
            Tipo = row.Tipo,
            Estado = row.Estado,
            FechaEnvio = DateTime.Parse(row.FechaEnvio, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
        };
    }
}