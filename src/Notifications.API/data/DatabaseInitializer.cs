// DatabaseInitializer crea las tablas de notifications.db cuando arranca la app.
// Si las tablas ya existen no hace nada (CREATE TABLE IF NOT EXISTS),
// así que se puede ejecutar en cada arranque sin perder datos.
using Microsoft.Data.Sqlite;

namespace Notifications.API.Data
{
    /// <summary>Crea el esquema de la base de Notifications si todavía no existe.</summary>
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Falta ConnectionStrings:DefaultConnection en appsettings.json");
        }

        /// <summary>Crea la tabla notifications. Se llama una sola vez desde Program.cs al arrancar.</summary>
        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // el Guid y la fecha se guardan como texto porque SQLite no tiene esos tipos
            // el índice en usuarioId acelera el listado por usuario (GET /api/notifications/{userId})
            var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE IF NOT EXISTS notifications (
                    id TEXT PRIMARY KEY,
                    usuarioId TEXT NOT NULL,
                    mensaje TEXT NOT NULL,
                    tipo TEXT NOT NULL,
                    estado TEXT NOT NULL,
                    fechaEnvio TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS idx_notifications_usuarioId ON notifications (usuarioId);
            """;
            command.ExecuteNonQuery();
        }
    }
}