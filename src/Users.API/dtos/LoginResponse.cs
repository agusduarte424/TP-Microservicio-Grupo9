namespace Users.API.DTOs
{
    /// <summary>Usuario autenticado. Solo los 4 campos que define la consigna.</summary>
    public class LoginResponse
    {
        /// <summary>Identificador único del usuario.</summary>
        public Guid Id { get; set; }

        /// <summary>Nombre del usuario.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Apellido del usuario.</summary>
        public string Apellido { get; set; } = string.Empty;

        /// <summary>Email del usuario.</summary>
        public string Email { get; set; } = string.Empty;
    }
}