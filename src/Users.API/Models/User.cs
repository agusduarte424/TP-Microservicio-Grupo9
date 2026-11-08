namespace Users.API.Models
{
    /// <summary>
    /// Modelo que representa a un usuario dentro del sistema (Apéndice A de la consigna).
    /// </summary>
    public class User
    {
        /// <summary>Identificador único del usuario.</summary>
        public Guid Id { get; set; }

        /// <summary>Nombre del usuario.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Apellido del usuario.</summary>
        public string Apellido { get; set; } = string.Empty;

        /// <summary>Email del usuario. Es único en todo el sistema.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Hash de la contraseña. Nunca se devuelve en ninguna respuesta.</summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>Fecha en la que se registró el usuario.</summary>
        public DateTime FechaRegistro { get; set; }

        /// <summary>False cuando el usuario está bloqueado.</summary>
        public bool Activo { get; set; } = true;

        /// <summary>Se incrementa en cada login fallido y se resetea al loguearse bien.</summary>
        public int IntentosFallidos { get; set; }
    }
}