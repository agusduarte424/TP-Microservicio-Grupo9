# Notifications.API

Puerto: http://localhost:5005
Base: notifications.db (SQLite, se crea sola al arrancar)

## Propósito

Registra y simula el envío de notificaciones a un usuario, y las lista. Valida que el
destinatario exista preguntándole a Users.API. No envía mensajes reales: el envío es simulado.

## Endpoints

| Método | Ruta | Qué hace | Respuestas |
|---|---|---|---|
| POST | /api/notifications/send | Registra y simula el envío de una notificación | 201, 400 (NTF-002), 404 (NTF-001), 500 (NTF-004) |
| GET | /api/notifications/{userId} | Lista las notificaciones de un usuario, la más reciente primero | 200, 404 (NTF-003), 500 (NTF-004) |

## Recorrido de una request

Ejemplo: `POST /api/notifications/send`.

1. `Program.cs` arma la cadena: Correlation ID, logging de requests, `UseExceptionHandler`, controladores.
2. `NotificationsController.Send` recibe el body. Si no cumple las Data Annotations de `SendNotificationRequest` (campos requeridos, mensaje de hasta 500 caracteres, tipo Email, Push o SMS), responde el 400 (NTF-002) desde `ErrorHandlingExtensions.cs` sin llegar al Service.
3. `NotificationService.Enviar` rechaza un `usuarioId` vacío y le pregunta a `IUsersClient.ExisteUsuario` si el destinatario existe.
4. `UsersClient` hace `GET api/users/{id}` a Users.API, con el mismo `X-Correlation-Id` del request. Un 404 devuelve `false`; otro código que no sea 2xx lanza una excepción.
5. Si el usuario no existe, el Service lanza NTF-001. Si existe, arma la notificación con estado `Enviada` y la guarda con `INotificationRepository.Guardar`.
6. `NotificationRepository` ejecuta el SQL con Dapper contra `notifications.db`.
7. El controlador devuelve 201 con `NotificationResponse`. Si algo falló, el handler correspondiente arma el JSON de error con `errorCode`, `errorMessage` y `correlationId`.

## Archivos clave

| Archivo | Qué hace | Por qué existe |
|---|---|---|
| `Controllers/NotificationsController.cs` | Los 2 endpoints | Recibe el pedido HTTP y devuelve el status |
| `Services/NotificationService.cs` | Envío simulado y listado | Concentra las reglas de negocio y arma el flujo completo |
| `Services/IUsersClient.cs` | Contrato para consultar a Users | El Service no depende de cómo se hace la llamada HTTP |
| `Services/UsersClient.cs` | Llamada a `GET /api/users/{id}` | Traduce el status de Users: 404 es "no existe", cualquier otro error corta el flujo |
| `Data/INotificationRepository.cs` | Contrato de persistencia | El Service no depende de SQLite |
| `Data/NotificationRepository.cs` | SQL con Dapper | Habla con la base, sin llamadas HTTP |
| `Data/DatabaseInitializer.cs` | Crea la tabla `notifications` y su índice | La base se arma sola al arrancar |
| `Exceptions/ErrorCodes.cs` | Constantes NTF-001 a NTF-004 | Evita strings sueltos |
| `ExceptionHandlers/*.cs` | Un handler para 404, uno para 400 y el genérico 500 | Respuestas con el contrato de la consigna |
| `DTOs/*.cs` | Request y response | Separan lo que entra y sale del modelo `Notification` |
| `Extensions/*.cs` | Logging, Correlation ID, errores y persistencia | Cada bloque transversal en su archivo |

## Llamadas a otros servicios

| Llama a | Para qué | Si falla |
|---|---|---|
| Users.API (`GET /api/users/{id}`) | Validar que el destinatario exista, solo en `POST /send` | 404 de Users: NTF-001 (404). Timeout, caída o 5xx: NTF-004 (500) y no se guarda nada |

- La URL de Users sale de `appsettings.json` (`Services:UsersApi`) y el timeout es de 5 segundos.
- `GET /{userId}` **no** llama a Users: se responde solo con la base local.
- El Correlation ID viaja en el header `X-Correlation-Id`: `CorrelationIdHandler` se agrega a todos los `HttpClient` desde `AddCorrelationId()`.

## Errores

| errorCode | HTTP | Cuándo se lanza | Dónde |
|---|---|---|---|
| NTF-001 | 404 | `POST /send` cuando Users responde 404 para el destinatario | `NotificationService.Enviar` |
| NTF-002 | 400 | `POST /send` con campos faltantes, tipo no reconocido, mensaje de más de 500 caracteres, `usuarioId` vacío o JSON malformado | `InvalidModelStateResponseFactory` en `ErrorHandlingExtensions.cs`; el `usuarioId` vacío en `NotificationService.Enviar` |
| NTF-003 | 404 | `GET /{userId}` cuando el usuario no tiene notificaciones registradas | `NotificationService.ListarPorUsuario` |
| NTF-004 | 500 | Cualquier error inesperado, incluida una falla al llamar a Users | `GlobalExceptionHandler` |

## Decisiones de diseño

- **Un 404 de Users es NTF-001 y cualquier otro fallo es NTF-004.** Si Users responde 404, sabemos que el usuario no existe. Si Users no responde, no sabemos si existe, así que es un error interno y no se le dice al cliente que el usuario no existe.
- **El usuario se valida antes de guardar.** Esta base no tiene clave foránea a los usuarios porque viven en otro servicio, así que sin esa consulta se podrían guardar notificaciones de usuarios inexistentes.
- **El estado `Enviada` es simulado.** No se manda ningún mensaje real: la notificación queda como `Enviada` apenas se registra y se deja un log.
- **Una lista vacía es un 404 (NTF-003).** La consigna define ese caso como error del catálogo ("el userId no tiene notificaciones registradas"), así que no se devuelve un 200 con `[]`.
- **`GET /{userId}` no consulta a Users.** La consigna solo asigna NTF-001 al POST. Por eso el GET sigue funcionando con Users caído, y también da NTF-003 para un usuario que no existe.
- **`UsuarioId` es `Guid?` en el DTO** para que `[Required]` detecte cuando no viene. Un Guid vacío explícito se rechaza en el Service.
- **El tipo distingue mayúsculas:** solo se aceptan `Email`, `Push` y `SMS` tal cual.
- **Se acepta un destinatario bloqueado.** `GET /api/users/{id}` responde 200 aunque el usuario esté bloqueado, y Notifications solo pregunta si existe.
- **Un solo handler para NTF-001 y NTF-003.** Los dos son 404 y comparten `NotFoundException`; el código lo elige quien la lanza.
- **Sin handler de 409:** el catálogo de Notifications no tiene reglas de negocio con ese status.

## Cómo probarlo

Con `Notifications.API.http` (en la carpeta del servicio) o desde Swagger cuando esté disponible. Users.API tiene que estar corriendo en el puerto 5002.

- **Caso exitoso:** registrar un usuario en Users, y `POST /api/notifications/send` con su `id`, un mensaje y tipo `Email` responde 201 con estado `Enviada`.
- **Caso de error:** el mismo POST con un `usuarioId` que no existe responde 404 con NTF-001.
- **Listado:** `GET /api/notifications/{userId}` con ese usuario responde 200, y con un usuario sin envíos responde 404 con NTF-003.
- **Falla de Users:** con Users apagado, el POST responde 500 con NTF-004 y no queda nada guardado.

## Preguntas de defensa

1. ¿Qué pasa si Users API está caída cuando se envía una notificación?
   La llamada lanza una excepción, el Service se corta antes de guardar y llega al handler genérico, que responde 500 con NTF-004. No queda ninguna notificación registrada.
2. ¿Por qué un 404 de Users es NTF-001 y un timeout es NTF-004?
   Un 404 es una respuesta de negocio: el usuario no existe. Un timeout o un 5xx significa que no pudimos verificarlo, así que es un error interno de este servicio. Mezclarlos le diría al cliente que el usuario no existe cuando en realidad no se pudo consultar.
3. ¿Cómo viaja el Correlation ID de Notifications a Users y dónde se ve?
   `CorrelationIdHandler` es un `DelegatingHandler` que `AddCorrelationId()` engancha a todos los `HttpClient`. Agrega el header `X-Correlation-Id` del request actual a la llamada saliente, y el mismo valor aparece en los logs de los dos servicios.
4. ¿Por qué `GET /{userId}` no valida que el usuario exista?
   Porque la consigna define el 404 de ese endpoint como "el userId no tiene notificaciones" (NTF-003) y asigna NTF-001 solo al POST. Responde solo con la base local, por eso funciona aunque Users esté caído.
5. ¿Por qué se valida al usuario antes de guardar la notificación?
   Porque esta base no tiene clave foránea hacia los usuarios (viven en otro servicio con su propia base). Sin esa consulta se podrían guardar notificaciones de usuarios inexistentes.
