namespace Orders.API.Exceptions
{
    /// <summary>Códigos de error del catálogo de Orders (sección 4.3 de la consigna).</summary>
    public static class ErrorCodes
    {
        public const string OrdenNoEncontrada = "ORD-001";
        public const string DatosInvalidos = "ORD-002";
        public const string UsuarioNoEncontrado = "ORD-003";
        public const string ProductoNoEncontrado = "ORD-004";
        public const string StockInsuficiente = "ORD-005";
        public const string TransicionInvalida = "ORD-006";
        public const string ErrorInterno = "ORD-007";
    }
}