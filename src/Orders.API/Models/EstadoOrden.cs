namespace Orders.API.Models
{
    /// <summary>Estados posibles de una orden, tal como los nombra la consigna.</summary>
    public static class EstadoOrden
    {
        public const string Pendiente = "Pendiente";
        public const string Confirmada = "Confirmada";
        public const string Enviada = "Enviada";
        public const string Entregada = "Entregada";
        public const string Cancelada = "Cancelada";


        /// <summary>Los 5 estados válidos.</summary>
        public static readonly string[] Todos = [Pendiente, Confirmada, Enviada, Entregada, Cancelada];

        // Transiciones permitidas. La consigna nombra los estados pero no define los cambios,
        // así que seguimos el ciclo real de un pedido: se cancela solo lo que todavía no se envió.
        // Entregada y Cancelada son finales: de ahí no se sale.
        private static readonly Dictionary<string, string[]> Siguientes = new()
        {
            [Pendiente] = [Confirmada, Cancelada],
            [Confirmada] = [Enviada, Cancelada],
            [Enviada] = [Entregada],
            [Entregada] = [],
            [Cancelada] = []
        };

        /// <summary>true si una orden puede pasar del estado actual al nuevo.</summary>
        public static bool PuedeCambiar(string actual, string nuevo) =>
            Siguientes.TryGetValue(actual, out var permitidos) && permitidos.Contains(nuevo);

        /// <summary>Devuelve el estado con el nombre exacto ("confirmada" → "Confirmada"), o null si no es uno de los 5.</summary>
        public static string? Normalizar(string? valor) =>
            Todos.FirstOrDefault(e => string.Equals(e, valor?.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}