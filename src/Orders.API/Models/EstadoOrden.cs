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
    }
}