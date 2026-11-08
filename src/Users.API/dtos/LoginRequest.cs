using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs
{
    /// <summary>Credenciales para iniciar sesión (POST /api/users/login).</summary>
    public class LoginRequest
    {
        /// <summary>Email con el que se registró el usuario.</summary>
        [Required(ErrorMessage = "El email es requerido.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Contraseña del usuario.</summary>
        [Required(ErrorMessage = "La contraseña es requerida.")]
        public string Password { get; set; } = string.Empty;
    }
}