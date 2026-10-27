namespace Products.API.Exceptions
{
    /// <summary>Códigos de error del catálogo de Products (sección 4.1 de la consigna).</summary>
    public static class ErrorCodes
    {
        public const string ProductoNoEncontrado = "PRD-001";
        public const string DatosInvalidos = "PRD-002";
        public const string ProductoDuplicado = "PRD-003";
        public const string ProductoConOrdenesActivas = "PRD-004";
        public const string ErrorInterno = "PRD-005";
    }
}