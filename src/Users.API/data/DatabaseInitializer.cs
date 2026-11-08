// DatabaseInitializer crea las tablas de users.db cuando arranca la app.
// Si las tablas ya existen no hace nada (CREATE TABLE IF NOT EXISTS),
// así que se puede ejecutar en cada arranque sin perder datos.
using Microsoft.Data.Sqlite;

namespace Users.API.Data
{
    /// <summary>Crea el esquema de la base de Users si todavía no existe.</summary>
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Falta ConnectionStrings:DefaultConnection en appsettings.json");
        }

        /// <summary>Crea la tabla users. Se llama una sola vez desde Program.cs al arrancar.</summary>
        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // el Guid y la fecha se guardan como texto y el bool como 0/1 porque SQLite no tiene esos tipos
            // email es UNIQUE y COLLATE NOCASE: no se repite aunque cambien las mayúsculas
            var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE IF NOT EXISTS users (
                    id TEXT PRIMARY KEY,
                    nombre TEXT NOT NULL,
                    apellido TEXT NOT NULL,
                    email TEXT NOT NULL UNIQUE COLLATE NOCASE,
                    passwordHash TEXT NOT NULL,
                    fechaRegistro TEXT NOT NULL,
                    activo INTEGER NOT NULL DEFAULT 1,
                    intentosFallidos INTEGER NOT NULL DEFAULT 0
                );
            """;
            command.ExecuteNonQuery();
        }
    }
}