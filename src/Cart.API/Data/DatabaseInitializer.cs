// Crea las tablas de cart.db al arrancar (si ya existen no hace nada).
using Microsoft.Data.Sqlite;

namespace Cart.API.Data
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection en appsettings.json");
        }

        /// <summary>Un carrito por usuario (carts) y sus productos (cart_items).</summary>
        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE IF NOT EXISTS carts (
                    usuarioId TEXT PRIMARY KEY,
                    fechaActualizacion TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS cart_items (
                    usuarioId TEXT NOT NULL,
                    productoId TEXT NOT NULL,
                    cantidad INTEGER NOT NULL,
                    PRIMARY KEY (usuarioId, productoId)
                );
            """;
            command.ExecuteNonQuery();
        }
    }
}