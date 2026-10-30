namespace Cart.API.Exceptions
{
    /// <summary>Códigos de error del catálogo de Cart (sección 4.4 de la consigna).</summary>
    public static class ErrorCodes
    {
        public const string CarritoNoEncontrado = "CRT-001";
        public const string ProductoNoEncontrado = "CRT-002";
        public const string StockInsuficiente = "CRT-003";
        public const string CantidadInvalida = "CRT-004";
        public const string ErrorInterno = "CRT-005";
    }
}