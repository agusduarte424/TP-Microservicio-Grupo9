// OrderRepository implementa IOrderRepository con SQLite y Dapper.
// Solo habla con orders.db: nunca llama a Users ni a Products (eso lo hace el Service).
using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Orders.API.Models;

namespace Orders.API.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection en appsettings.json");
        }

        // una conexión nueva por operación; SQLite es un archivo local, abrirla es barato
        private SqliteConnection CreateConnection() => new(_connectionString);

        public async Task<IEnumerable<Order>> ObtenerTodos(Guid? usuarioId)
        {
            using var conn = CreateConnection();

            // el filtro se agrega solo si viene, siempre como parámetro (nunca concatenando el valor)
            var sql = "SELECT * FROM orders";
            if (usuarioId.HasValue)
                sql += " WHERE usuarioId = @UsuarioId";
            sql += " ORDER BY fechaCreacion DESC";

            var rows = await conn.QueryAsync<OrderRow>(sql, new { UsuarioId = usuarioId?.ToString() });

            // se traen los items de una sola vez y se agrupan por orden
            var items = await conn.QueryAsync<OrderItemRow>("SELECT * FROM order_items");
            var itemsPorOrden = items.ToLookup(i => i.OrderId);

            return rows.Select(r => ToOrder(r, itemsPorOrden[r.Id])).ToList();
        }

        public async Task<Order?> ObtenerPorId(Guid id)
        {
            using var conn = CreateConnection();

            var row = await conn.QueryFirstOrDefaultAsync<OrderRow>(
                "SELECT * FROM orders WHERE id = @Id", new { Id = id.ToString() });

            if (row is null)
                return null;

            var items = await conn.QueryAsync<OrderItemRow>(
                "SELECT * FROM order_items WHERE orderId = @Id", new { Id = row.Id });

            return ToOrder(row, items);
        }

        // convierte las filas de SQLite (texto y números) en el modelo Order
        private static Order ToOrder(OrderRow row, IEnumerable<OrderItemRow> items) => new()
        {
            Id = Guid.Parse(row.Id),
            UsuarioId = Guid.Parse(row.UsuarioId),
            Total = (decimal)row.Total,
            Estado = row.Estado,
            FechaCreacion = ParseFecha(row.FechaCreacion),
            FechaActualizacion = row.FechaActualizacion is null ? null : ParseFecha(row.FechaActualizacion),
            Items = items.Select(i => new OrderItem
            {
                ProductoId = Guid.Parse(i.ProductoId),
                Cantidad = (int)i.Cantidad,
                PrecioUnitario = (decimal)i.PrecioUnitario
            }).ToList()
        };

        private static DateTime ParseFecha(string valor) =>
            DateTime.Parse(valor, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        // filas tal como vienen de SQLite: Guid y fechas como texto, números como double y long
        private sealed class OrderRow
        {
            public string Id { get; set; } = string.Empty;
            public string UsuarioId { get; set; } = string.Empty;
            public double Total { get; set; }
            public string Estado { get; set; } = string.Empty;
            public string FechaCreacion { get; set; } = string.Empty;
            public string? FechaActualizacion { get; set; }
        }

        private sealed class OrderItemRow
        {
            public string OrderId { get; set; } = string.Empty;
            public string ProductoId { get; set; } = string.Empty;
            public long Cantidad { get; set; }
            public double PrecioUnitario { get; set; }
        }
    }
}