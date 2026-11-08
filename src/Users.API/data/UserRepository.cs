// UserRepository implementa IUserRepository con SQLite y Dapper:
// el UserService le pide que guarde o busque usuarios
// y el Repository se encarga de hablar con la base users.db.
using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Users.API.Models;

namespace Users.API.Data
{
    /// <summary>Persistencia de usuarios en SQLite usando Dapper.</summary>
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        // la cadena de conexión viene de appsettings.json → ConnectionStrings:DefaultConnection
        // si falta, se corta al arrancar en vez de crear una base en un lugar inesperado
        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Falta ConnectionStrings:DefaultConnection en appsettings.json");
        }

        // una conexión nueva por operación; SQLite es un archivo local, abrirla es barato
        private SqliteConnection CreateConnection() => new(_connectionString);

        public async Task<User?> ObtenerPorId(Guid id)
        {
            using var conn = CreateConnection();

            var row = await conn.QueryFirstOrDefaultAsync<UserRow>(
                "SELECT * FROM users WHERE id = @Id",
                new { Id = id.ToString() });

            return row is null ? null : ToUser(row);
        }

        public async Task<User?> ObtenerPorEmail(string email)
        {
            using var conn = CreateConnection();

            // la columna email es COLLATE NOCASE, así que "Maria@x.com" y "maria@x.com" coinciden
            var row = await conn.QueryFirstOrDefaultAsync<UserRow>(
                "SELECT * FROM users WHERE email = @Email",
                new { Email = email });

            return row is null ? null : ToUser(row);
        }

        public async Task<bool> ExistePorEmail(string email)
        {
            using var conn = CreateConnection();

            var count = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM users WHERE email = @Email",
                new { Email = email });

            return count > 0;
        }

        public async Task Guardar(User user)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync("""
                INSERT INTO users (id, nombre, apellido, email, passwordHash, fechaRegistro, activo, intentosFallidos)
                VALUES (@Id, @Nombre, @Apellido, @Email, @PasswordHash, @FechaRegistro, @Activo, @IntentosFallidos)
            """, new
            {
                Id = user.Id.ToString(),
                user.Nombre,
                user.Apellido,
                user.Email,
                user.PasswordHash,
                FechaRegistro = user.FechaRegistro.ToString("o", CultureInfo.InvariantCulture),
                Activo = user.Activo ? 1 : 0,
                user.IntentosFallidos
            });
        }

        public async Task ActualizarEstadoLogin(Guid id, int intentosFallidos, bool activo)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync("""
                UPDATE users
                SET intentosFallidos = @IntentosFallidos,
                    activo = @Activo
                WHERE id = @Id
            """, new
            {
                Id = id.ToString(),
                IntentosFallidos = intentosFallidos,
                Activo = activo ? 1 : 0
            });
        }

        // SQLite guarda el Guid y la fecha como texto y el bool como entero:
        // Dapper llena esta fila con los tipos de la base y ToUser la pasa al modelo
        private sealed class UserRow
        {
            public string Id { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Apellido { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
            public string FechaRegistro { get; set; } = string.Empty;
            public long Activo { get; set; }
            public long IntentosFallidos { get; set; }
        }

        private static User ToUser(UserRow row) => new()
        {
            Id = Guid.Parse(row.Id),
            Nombre = row.Nombre,
            Apellido = row.Apellido,
            Email = row.Email,
            PasswordHash = row.PasswordHash,
            FechaRegistro = DateTime.Parse(row.FechaRegistro, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            Activo = row.Activo == 1,
            IntentosFallidos = (int)row.IntentosFallidos
        };
    }
}