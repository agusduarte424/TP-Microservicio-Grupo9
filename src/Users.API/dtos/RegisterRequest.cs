using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs
{
    /// <summary>Datos para registrar un usuario (POST /api/users/register).</summary>
    public class RegisterRequest
    {
        /// <summary>Nombre del usuario. Requerido.</summary>
        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Apellido del usuario. Requerido.</summary>
        [Required(ErrorMessage = "El apellido es requerido.")]
        [StringLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
        public string Apellido { get; set; } = string.Empty;

        /// <summary>Email del usuario. Requerido, con formato válido y único en el sistema.</summary>
        [Required(ErrorMessage = "El email es requerido.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        [StringLength(200, ErrorMessage = "El email no puede superar los 200 caracteres.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Contraseña. Requerida, de 8 a 100 caracteres, con al menos una letra y un número.</summary>
        [Required(ErrorMessage = "La contraseña es requerida.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "La contraseña debe tener al menos una letra y un número.")]
        public string Password { get; set; } = string.Empty;
    }
}