namespace Users.API.DTOs
{
    /// <summary>Usuario recién registrado. No incluye la contraseña ni su hash.</summary>
    public class RegisterResponse
    {
        /// <summary>Identificador único del usuario.</summary>
        public Guid Id { get; set; }

        /// <summary>Nombre del usuario.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Apellido del usuario.</summary>
        public string Apellido { get; set; } = string.Empty;

        /// <summary>Email del usuario.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Fecha en la que se registró el usuario.</summary>
        public DateTime FechaRegistro { get; set; }

        /// <summary>False si el usuario está bloqueado.</summary>
        public bool Activo { get; set; }
    }
}