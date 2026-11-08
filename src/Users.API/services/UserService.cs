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

        // al llegar a esta cantidad de intentos fallidos consecutivos, el usuario se bloquea
        private const int MaxIntentosFallidos = 3;

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

        /// <summary>
        /// Autentica a un usuario. USR-003 si las credenciales no coinciden,
        /// USR-004 si está bloqueado por intentos fallidos, USR-005 si está bloqueado por fraude.
        /// </summary>
        public async Task<LoginResponse> Login(LoginRequest request)
        {
            var user = await _repository.ObtenerPorEmail(request.Email.Trim());

            // mismo error que con una contraseña incorrecta: no revela si el email existe
            if (user is null)
                throw new UnauthorizedException(ErrorCodes.CredencialesIncorrectas, "Credenciales incorrectas.");

            // un usuario bloqueado no entra ni con la contraseña correcta
            if (!user.Activo)
            {
                // bloqueado con 3 o más intentos = lo bloqueó el sistema (USR-004);
                // bloqueado con menos = alguien lo bloqueó a mano por fraude (USR-005)
                if (user.IntentosFallidos >= MaxIntentosFallidos)
                    throw new ForbiddenException(ErrorCodes.UsuarioBloqueadoPorIntentos,
                        "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");

                throw new ForbiddenException(ErrorCodes.UsuarioBloqueadoPorFraude,
                    "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.");
            }

            if (!PasswordHasher.Verificar(request.Password, user.PasswordHash))
            {
                var intentos = user.IntentosFallidos + 1;
                var sigueActivo = intentos < MaxIntentosFallidos;
                await _repository.ActualizarEstadoLogin(user.Id, intentos, sigueActivo);

                // el tercer intento fallido ya responde como bloqueado
                if (!sigueActivo)
                    throw new ForbiddenException(ErrorCodes.UsuarioBloqueadoPorIntentos,
                        "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");

                throw new UnauthorizedException(ErrorCodes.CredencialesIncorrectas, "Credenciales incorrectas.");
            }

            // login correcto: los intentos fallidos vuelven a cero (solo se escribe si hacía falta)
            if (user.IntentosFallidos > 0)
                await _repository.ActualizarEstadoLogin(user.Id, 0, true);

            return new LoginResponse
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email
            };
        }

        /// <summary>
        /// Devuelve un usuario por id, o null si no existe.
        /// Devuelve null en vez de lanzar una excepción porque el catálogo de Users no tiene un código
        /// de "no encontrado": el controlador responde 404 y el servicio que llama lo interpreta.
        /// </summary>
        public async Task<UserResponse?> ObtenerPorId(Guid id)
        {
            var user = await _repository.ObtenerPorId(id);
            if (user is null) return null;

            return new UserResponse
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email,
                Activo = user.Activo
            };
        }
    }
}