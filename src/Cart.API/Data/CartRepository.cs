// CartRepository implementa ICartRepository con SQLite y Dapper.
// Solo habla con cart.db: nunca llama a Products (eso lo hace el Service).
using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Cart.API.Models;
using CartModel = Cart.API.Models.Cart;

namespace Cart.API.Data
{
    public class CartRepository : ICartRepository
    {
        private readonly string _connectionString;

        public CartRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection en appsettings.json");
        }

        private SqliteConnection CreateConnection() => new(_connectionString);

        public async Task<CartModel?> ObtenerPorUsuario(Guid usuarioId)
        {
            using var conn = CreateConnection();
            var id = usuarioId.ToString();

            var fecha = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT fechaActualizacion FROM carts WHERE usuarioId = @Id", new { Id = id });

            if (fecha is null)
                return null;

            var items = await conn.QueryAsync<CartItemRow>(
                "SELECT productoId, cantidad FROM cart_items WHERE usuarioId = @Id", new { Id = id });

            return new CartModel
            {
                UsuarioId = usuarioId,
                FechaActualizacion = DateTime.Parse(fecha, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                Items = items.Select(i => new CartItem
                {
                    ProductoId = Guid.Parse(i.ProductoId),
                    Cantidad = (int)i.Cantidad
                }).ToList()
            };
        }

        public async Task Guardar(CartModel cart)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            // todo en una transacción: o se guarda el carrito completo o no se guarda nada
            using var tx = conn.BeginTransaction();
            var id = cart.UsuarioId.ToString();

            // si el carrito no existe lo crea; si existe, actualiza la fecha
            await conn.ExecuteAsync("""
                INSERT INTO carts (usuarioId, fechaActualizacion) VALUES (@Id, @Fecha)
                ON CONFLICT(usuarioId) DO UPDATE SET fechaActualizacion = excluded.fechaActualizacion
            """, new { Id = id, Fecha = cart.FechaActualizacion.ToString("o", CultureInfo.InvariantCulture) }, tx);

            // se borran los items viejos y se insertan los actuales
            await conn.ExecuteAsync("DELETE FROM cart_items WHERE usuarioId = @Id", new { Id = id }, tx);

            await conn.ExecuteAsync(
                "INSERT INTO cart_items (usuarioId, productoId, cantidad) VALUES (@Id, @ProductoId, @Cantidad)",
                cart.Items.Select(i => new { Id = id, ProductoId = i.ProductoId.ToString(), i.Cantidad }),
                tx);

            tx.Commit();
        }

        // fila de cart_items tal como viene de SQLite (texto y entero largo)
        private sealed class CartItemRow
        {
            public string ProductoId { get; set; } = string.Empty;
            public long Cantidad { get; set; }
        }
    }
}