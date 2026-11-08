// UserService tiene la lógica de negocio de Users.
// Solo usa IUserRepository para los datos y lanza las excepciones del catálogo.
using Users.API.Data;
using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Services
{
    public class UserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        /// <summary>Registra un usuario. Lanza USR-001 si el email ya está registrado.</summary>
        public async Task<RegisterResponse> Registrar(RegisterRequest request)
        {
            var email = request.Email.Trim();

            // se chequea antes de hashear, para no gastar tiempo de cálculo si igual va a fallar
            if (await _repository.ExistePorEmail(email))
                throw new BusinessRuleException(ErrorCodes.EmailYaRegistrado,
                    $"El email '{email}' ya está registrado.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre.Trim(),
                Apellido = request.Apellido.Trim(),
                Email = email,
                // se guarda solo el hash: la contraseña original no se almacena en ningún lado
                PasswordHash = PasswordHasher.Hash(request.Password),
                FechaRegistro = DateTime.UtcNow,
                Activo = true,
                IntentosFallidos = 0
            };

            await _repository.Guardar(user);

            return new RegisterResponse
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email,
                FechaRegistro = user.FechaRegistro,
                Activo = user.Activo
            };
        }
    }
}