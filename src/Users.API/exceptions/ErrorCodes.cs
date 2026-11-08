namespace Users.API.Exceptions
{
    /// <summary>Códigos de error del catálogo de Users (sección 4.2 de la consigna).</summary>
    public static class ErrorCodes
    {
        public const string EmailYaRegistrado = "USR-001";
        public const string DatosInvalidos = "USR-002";
        public const string CredencialesIncorrectas = "USR-003";
        public const string UsuarioBloqueadoPorIntentos = "USR-004";
        public const string UsuarioBloqueadoPorFraude = "USR-005";
        public const string ErrorInterno = "USR-006";
    }
}