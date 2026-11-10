// Crea las tablas de orders.db al arrancar. Si ya existen no hace nada
// (CREATE TABLE IF NOT EXISTS), así que se puede ejecutar en cada arranque sin perder datos.
using Microsoft.Data.Sqlite;

namespace Orders.API.Data
{
    /// <summary>Crea el esquema de la base de Orders si todavía no existe.</summary>
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection en appsettings.json");
        }

        /// <summary>Una fila por orden (orders) y una por cada producto de la orden (order_items).</summary>
        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Guid y fechas como texto, porque SQLite no tiene esos tipos.
            // Los precios como REAL, igual que en Products.
            var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE IF NOT EXISTS orders (
                    id TEXT PRIMARY KEY,
                    usuarioId TEXT NOT NULL,
                    total REAL NOT NULL,
                    estado TEXT NOT NULL,
                    fechaCreacion TEXT NOT NULL,
                    fechaActualizacion TEXT
                );
                CREATE TABLE IF NOT EXISTS order_items (
                    orderId TEXT NOT NULL,
                    productoId TEXT NOT NULL,
                    cantidad INTEGER NOT NULL,
                    precioUnitario REAL NOT NULL,
                    PRIMARY KEY (orderId, productoId)
                );
            """;
            command.ExecuteNonQuery();
        }
    }
}