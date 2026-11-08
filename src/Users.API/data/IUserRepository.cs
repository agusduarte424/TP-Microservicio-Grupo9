// IUserRepository es el contrato de persistencia de Users:
// el UserService solo conoce esta interfaz y no sabe si atrás hay SQLite u otra base.
// Ningún repositorio hace llamadas HTTP: eso es trabajo del Service.
using Users.API.Models;

namespace Users.API.Data
{
    /// <summary>Operaciones de persistencia de usuarios.</summary>
    public interface IUserRepository
    {
        /// <summary>Devuelve el usuario con ese id, o null si no existe.</summary>
        Task<User?> ObtenerPorId(Guid id);

        /// <summary>Devuelve el usuario con ese email (sin distinguir mayúsculas), o null si no existe.</summary>
        Task<User?> ObtenerPorEmail(string email);

        /// <summary>Indica si ya hay un usuario registrado con ese email.</summary>
        Task<bool> ExistePorEmail(string email);

        /// <summary>Inserta un usuario nuevo.</summary>
        Task Guardar(User user);

        /// <summary>Actualiza los intentos fallidos y si el usuario está activo. Lo usa el login.</summary>
        Task ActualizarEstadoLogin(Guid id, int intentosFallidos, bool activo);
    }
}