# Users.API

Puerto: http://localhost:5002
Base: users.db (SQLite, se crea sola al arrancar)

## Propósito

Registra usuarios y los autentica, con bloqueo de la cuenta a los 3 intentos fallidos.
No llama a ningún otro servicio. Otros servicios le preguntan si un usuario existe.

## Endpoints

| Método | Ruta | Qué hace | Respuestas |
|---|---|---|---|
| POST | /api/users/register | Registra un usuario | 201, 400 (USR-002), 409 (USR-001), 500 (USR-006) |
| POST | /api/users/login | Autentica con email y contraseña | 200, 400 (USR-002), 401 (USR-003), 403 (USR-004, USR-005), 500 (USR-006) |
| GET | /api/users/{id} | **Interno.** Lo usan Orders y Notifications para verificar que el usuario existe | 200, 404 (sin errorCode), 500 (USR-006) |

`PasswordHash` nunca viaja en una respuesta: cada endpoint devuelve su propio DTO de salida.

## Recorrido de una request

Ejemplo: `POST /api/users/login`.

1. `Program.cs` arma la cadena: Correlation ID, logging de requests, `UseExceptionHandler`, controladores.
2. `UsersController.Login` recibe el body. Si no cumple las Data Annotations de `LoginRequest`, responde el 400 (USR-002) desde `ErrorHandlingExtensions.cs` sin llegar al Service.
3. `UserService.Login` pide el usuario a `IUserRepository.ObtenerPorEmail`, revisa el bloqueo, verifica la contraseña con `PasswordHasher.Verificar` y actualiza los intentos.
4. `UserRepository` ejecuta el SQL con Dapper contra `users.db`.
5. Si algo falla, el Service lanza una excepción del catálogo y el handler correspondiente arma el JSON de error con `errorCode`, `errorMessage` y `correlationId`.
6. Si sale bien, el controlador devuelve 200 con `LoginResponse`.

## Archivos clave

| Archivo | Qué hace | Por qué existe |
|---|---|---|
| `Controllers/UsersController.cs` | Los 3 endpoints | Recibe el pedido HTTP y devuelve el status |
| `Services/UserService.cs` | Registro, login, bloqueo, búsqueda por id | Concentra las reglas de negocio |
| `Services/PasswordHasher.cs` | Hash y verificación con PBKDF2 | Nunca se guarda la contraseña, solo su hash |
| `Data/IUserRepository.cs` | Contrato de persistencia | El Service no depende de SQLite |
| `Data/UserRepository.cs` | SQL con Dapper | Habla con la base, sin llamadas HTTP |
| `Data/DatabaseInitializer.cs` | Crea la tabla `users` | La base se arma sola al arrancar |
| `Exceptions/ErrorCodes.cs` | Constantes USR-001 a USR-006 | Evita strings sueltos |
| `ExceptionHandlers/*.cs` | Un handler por tipo de excepción y el genérico | Un 400, 401, 403, 409 y 500 con el contrato de la consigna |
| `DTOs/*.cs` | Request y response de cada endpoint | Separan lo que entra y sale del modelo `User` |
| `Extensions/*.cs` | Logging, Correlation ID, errores y persistencia | Cada bloque transversal en su archivo |

## Llamadas a otros servicios

Ninguna. Users es un servicio del que dependen otros: Orders lo llama para ORD-003 y
Notifications para NTF-001.

## Errores

| errorCode | HTTP | Cuándo se lanza | Dónde |
|---|---|---|---|
| USR-001 | 409 | Registro con un email ya registrado (sin distinguir mayúsculas) | `UserService.Registrar` |
| USR-002 | 400 | Registro o login con campos faltantes o formato incorrecto, o JSON malformado | `InvalidModelStateResponseFactory` en `ErrorHandlingExtensions.cs` |
| USR-003 | 401 | Email inexistente o contraseña incorrecta (menos de 3 intentos) | `UserService.Login` |
| USR-004 | 403 | Cuenta bloqueada con 3 o más intentos fallidos, o el tercer intento fallido | `UserService.Login` |
| USR-005 | 403 | Cuenta bloqueada manualmente (`activo = 0` con menos de 3 intentos) | `UserService.Login` |
| USR-006 | 500 | Cualquier error inesperado | `GlobalExceptionHandler` |

## Decisiones de diseño

- **Bloqueo a los 3 intentos.** Cada contraseña incorrecta suma 1 a `IntentosFallidos`. Al llegar a 3, `Activo` pasa a `false`. Un login correcto lo vuelve a 0.
- **USR-004 contra USR-005 sin columna extra.** Con `Activo = false` e `IntentosFallidos >= 3` es bloqueo por intentos; con menos de 3 es bloqueo manual. Se descartó agregar una columna `motivoBloqueo` porque la consigna no la define.
- **El bloqueo se revisa antes que la contraseña.** Una cuenta bloqueada no entra ni con la contraseña correcta, y no se le deja probar más.
- **El mismo 401 si el email no existe.** Para no revelar qué emails están registrados.
- **La validación de formato usa USR-002.** Es el único código 400 del catálogo, así que el login también lo usa para un body inválido.
- **Reglas de la contraseña.** La consigna no las define. Se eligió de 8 a 100 caracteres con al menos una letra y un número.
- **`GET /api/users/{id}` es interno.** La consigna no lo define, pero Orders y Notifications lo necesitan. Cuando no existe responde 404 **sin `errorCode`**, porque el catálogo de Users no tiene un código de "no encontrado" y no se inventan códigos. Los servicios que lo llaman solo miran el status.
- **El mismo endpoint responde 200 aunque el usuario esté bloqueado.** Informa el estado (`activo`) y no decide. Si el grupo quiere que Orders rechace a un usuario bloqueado, se hace en Orders con ese campo.
- **Hash con PBKDF2** (`Rfc2898DeriveBytes`, SHA-256, 100.000 iteraciones y sal aleatoria), que viene en .NET y no necesita paquetes.

## Cómo probarlo

Con `Users.API.http` (en la carpeta del servicio) o desde Swagger cuando esté disponible.

- **Caso exitoso:** `POST /api/users/register` con un usuario válido responde 201 sin password ni hash.
- **Caso de error:** el mismo registro por segunda vez responde 409 con USR-001.
- **Bloqueo:** `POST /api/users/login` con contraseña incorrecta tres veces da 401 (USR-003), 401 (USR-003) y 403 (USR-004).
- **USR-005:** requiere bloquear a mano al usuario. Abrir `users.db` con DB Browser for SQLite y poner `activo = 0` y `intentosFallidos = 0` en esa fila.

## Preguntas de defensa

1. ¿Cómo y cuándo se bloquea un usuario, y qué distingue a USR-004 de USR-005?
   Cada login con contraseña incorrecta suma 1 a `IntentosFallidos`. Al llegar a 3, `Activo` pasa a `false` y responde USR-004. USR-005 también es un 403 con `Activo = false`, pero con menos de 3 intentos: es el bloqueo manual por fraude.
2. ¿Por qué se guarda un hash y no la contraseña?
   Porque un hash no se puede revertir: si roban la base, no ven contraseñas. PBKDF2 con sal aleatoria y 100.000 iteraciones hace que dos usuarios con la misma contraseña tengan hashes distintos y que probar contraseñas sea lento.
3. ¿Por qué el login devuelve 401 con el mismo mensaje si el email no existe?
   Para no revelar qué emails están registrados: quien intenta entrar ve el mismo error sin importar cuál de los dos datos falló.
4. ¿Por qué USR-003 es 401 y USR-004 es 403?
   El 401 significa que no se pudo verificar quién sos (credenciales que no coinciden). El 403 significa que se sabe quién sos pero la cuenta tiene el acceso bloqueado.
5. ¿Por qué existe `GET /api/users/{id}` si la consigna no lo define?
   Porque Orders y Notifications deben validar que el usuario exista y cada servicio tiene su propia base. Responde 404 sin `errorCode` porque el catálogo de Users no tiene un código de "no encontrado".
