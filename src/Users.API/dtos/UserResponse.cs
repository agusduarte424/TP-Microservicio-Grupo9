namespace Users.API.DTOs
{
    /// <summary>Datos públicos de un usuario. Sin contraseña ni hash.</summary>
    public class UserResponse
    {
        /// <summary>Identificador único del usuario.</summary>
        public Guid Id { get; set; }

        /// <summary>Nombre del usuario.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Apellido del usuario.</summary>
        public string Apellido { get; set; } = string.Empty;

        /// <summary>Email del usuario.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>False si el usuario está bloqueado.</summary>
        public bool Activo { get; set; }
    }
}