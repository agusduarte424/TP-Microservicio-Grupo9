namespace Notifications.API.Exceptions
{
    /// <summary>Códigos de error del catálogo de Notifications (sección 4.5 de la consigna).</summary>
    public static class ErrorCodes
    {
        public const string UsuarioNoEncontrado = "NTF-001";
        public const string DatosInvalidos = "NTF-002";
        public const string SinNotificaciones = "NTF-003";
        public const string ErrorInterno = "NTF-004";
    }
}