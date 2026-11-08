# TP Microservicios E-Commerce · Grupo 9

Trabajo práctico de **Arquitectura y Diseño de Software**. Un sistema de E-Commerce armado con
5 microservicios independientes, cada uno una REST API en C# con .NET 10 y su propia base de datos.

## Integrantes y responsables

| Integrante | Servicios |
|---|---|
| Agus | Products, Cart |
| Pili | Users, Notifications |
| Rama | Orders |

Los aspectos transversales (errores, logging, Correlation ID, Swagger y health checks) se
construyeron como bloques reutilizables y los incorpora cada servicio.

## Tecnologías

| Propósito | Tecnología |
|---|---|
| Framework | .NET 10 / ASP.NET Core |
| Documentación de la API | Swashbuckle.AspNetCore 6.6.2 (Swagger UI) |
| Logging | Serilog: consola legible y archivo JSON |
| Persistencia | SQLite con Dapper (ver la nota de persistencia más abajo) |
| Validación | Data Annotations |
| Manejo de errores | `IExceptionHandler` con `ProblemDetails` |
| HTTP entre servicios | `IHttpClientFactory` |

## Estructura del repositorio

```
ECommerce.slnx
├── src/
│   ├── Products.API/
│   ├── Users.API/
│   ├── Orders.API/
│   ├── Cart.API/
│   └── Notifications.API/
└── docs/
```

Cada servicio sigue la misma estructura, con Products como plantilla:

```
<Servicio>.API/
├── Controllers/         Reciben el pedido HTTP y devuelven el status
├── Models/              Entidades del dominio
├── DTOs/                Request y response de cada endpoint
├── Services/            Lógica de negocio y clientes HTTP hacia otros servicios
├── Data/                Repositorios con SQLite y Dapper, detrás de una interfaz
├── Exceptions/          ErrorCodes y excepciones de dominio
├── ExceptionHandlers/   Un IExceptionHandler por tipo de excepción
├── Extensions/          Bloques transversales (logging, Correlation ID, errores, Swagger)
├── logs/                Archivos de log de Serilog (no se versionan)
└── Program.cs           Solo compone los bloques
```

## Servicios y puertos

| Servicio | Puerto | Responsabilidad | Base de datos | Swagger |
|---|---|---|---|---|
| Products.API | 5001 | Catálogo de productos | `products.db` | http://localhost:5001/swagger |
| Users.API | 5002 | Registro y login de usuarios, con bloqueo de cuenta | `users.db` | http://localhost:5002/swagger |
| Orders.API | 5003 | Órdenes de compra y sus estados | `orders.db` | http://localhost:5003/swagger |
| Cart.API | 5004 | Carrito de compras por usuario | `cart.db` | http://localhost:5004/swagger |
| Notifications.API | 5005 | Registro y simulación de notificaciones | `notifications.db` | http://localhost:5005/swagger |

Los puertos y las URLs entre servicios están en el `appsettings.json` de cada uno (secciones
`Urls` y `Services`) y no se escriben en el código.

## Arquitectura

Cada servicio es independiente y tiene su propia base de datos. Se comunican solo por HTTP,
usando las URLs de la sección `Services` de su `appsettings.json`.

```mermaid
flowchart LR
    cliente([Cliente / Swagger UI])

    subgraph servicios [Microservicios]
        P["Products.API<br/>:5001"]
        U["Users.API<br/>:5002"]
        O["Orders.API<br/>:5003"]
        C["Cart.API<br/>:5004"]
        N["Notifications.API<br/>:5005"]
    end

    cliente --> P & U & O & C & N

    O -->|valida el usuario| U
    O -->|valida producto, stock y precio| P
    P -->|consulta órdenes activas antes de borrar| O
    C -->|valida producto y stock| P
    N -->|valida el destinatario| U
```

<details>
<summary>Versión en texto del diagrama</summary>

```
 Cliente / Swagger UI
    |
    +--> Products.API :5001 ----(órdenes activas, antes de borrar)----> Orders.API
    |
    +--> Users.API :5002           (no llama a ningún servicio)
    |
    +--> Orders.API :5003 ------(valida el usuario)-------------------> Users.API
    |                     +-----(valida producto, stock y precio)-----> Products.API
    |
    +--> Cart.API :5004 --------(valida producto y stock)-------------> Products.API
    |
    +--> Notifications.API :5005 --(valida el destinatario)-----------> Users.API
```

</details>

### Llamadas entre servicios

| Servicio | Llama a | Para qué | Si falla |
|---|---|---|---|
| Cart.API | Products.API | Validar producto y stock (CRT-002, CRT-003) | 500 con CRT-005 |
| Orders.API | Users.API | Validar que el usuario exista (ORD-003) | 500 con ORD-007 |
| Orders.API | Products.API | Validar producto, stock y precio (ORD-004, ORD-005) | 500 con ORD-007 |
| Notifications.API | Users.API | Validar al destinatario (NTF-001) | 500 con NTF-004 |
| Products.API | Orders.API | Verificar órdenes activas antes de borrar (PRD-004) | 500 con PRD-005 |
| Users.API | Ninguno | No depende de otros servicios | No aplica |

Reglas que siguen todas las llamadas:

- **Un 404 del servicio remoto es el error de negocio del servicio que llama.** Por ejemplo, si
  Users responde 404 para un usuario, Orders devuelve ORD-003 y Notifications devuelve NTF-001.
- **Cualquier otro fallo** (timeout, servicio caído, 5xx) es un 500 con el código interno del
  servicio que llama, y la operación no se completa.
- **El status HTTP se verifica siempre antes de procesar la respuesta.**
- **El Correlation ID viaja en el header `X-Correlation-Id`** en cada llamada saliente, y aparece
  en los logs de ambos servicios y en los errores.

## Códigos de error

Todas las respuestas de error (4xx y 5xx) tienen la misma estructura. `errorCode` y
`errorMessage` salen del catálogo de cada servicio, y `correlationId` permite rastrear el error
en los logs:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "El recurso solicitado no fue encontrado.",
  "instance": "/api/products/99",
  "errorCode": "PRD-001",
  "errorMessage": "Producto no encontrado.",
  "correlationId": "0f8fad5b-d9cb-469f-a165-70867728950e"
}
```

En los errores de validación con varios campos, `errorMessage` lista todos los problemas
separados por punto y coma. Nunca se devuelve un stack trace.

### Products.API

| errorCode | HTTP | Mensaje | Cuándo se devuelve |
|---|---|---|---|
| PRD-001 | 404 | Producto no encontrado. | GET, PUT o DELETE cuando el ID no existe |
| PRD-002 | 400 | Los datos del producto son inválidos. | POST o PUT con campos faltantes o formato incorrecto |
| PRD-003 | 409 | Ya existe un producto con ese nombre en la categoría. | POST cuando se intenta crear un duplicado |
| PRD-004 | 409 | El producto tiene órdenes activas y no puede eliminarse. | DELETE cuando hay órdenes Pendiente o Confirmada que lo referencian |
| PRD-005 | 500 | Error interno al procesar el producto. | Error inesperado en el servicio o la persistencia |

### Users.API

| errorCode | HTTP | Mensaje | Cuándo se devuelve |
|---|---|---|---|
| USR-001 | 409 | El email ya está registrado. | POST /register cuando el email ya existe |
| USR-002 | 400 | Los datos del usuario son inválidos. | POST /register o /login con campos faltantes o formato incorrecto |
| USR-003 | 401 | Credenciales incorrectas. | POST /login cuando el email o la contraseña no coinciden |
| USR-004 | 403 | Usuario bloqueado por demasiados intentos fallidos. | POST /login con 3 o más intentos fallidos acumulados |
| USR-005 | 403 | Usuario bloqueado por detección de fraude. | POST /login cuando el usuario fue bloqueado manualmente |
| USR-006 | 500 | Error interno al procesar el usuario. | Error inesperado en el servicio o la persistencia |

El endpoint interno `GET /api/users/{id}` (lo usan Orders y Notifications) responde 404 sin
`errorCode` cuando el usuario no existe, porque el catálogo de Users no define un código de
"no encontrado".

### Orders.API

| errorCode | HTTP | Mensaje | Cuándo se devuelve |
|---|---|---|---|
| ORD-001 | 404 | Orden no encontrada. | GET o PUT cuando el ID de la orden no existe |
| ORD-002 | 400 | Los datos de la orden son inválidos. | POST con campos faltantes o lista de items vacía |
| ORD-003 | 404 | Usuario no encontrado al crear la orden. | POST cuando el UsuarioId no existe en Users API |
| ORD-004 | 404 | Producto no encontrado al crear la orden. | POST cuando algún ProductoId no existe en Products API |
| ORD-005 | 422 | Stock insuficiente para uno o más productos. | POST cuando la cantidad solicitada supera el stock disponible |
| ORD-006 | 409 | El estado de la orden no puede ser modificado. | PUT /status cuando la transición de estado no es válida |
| ORD-007 | 500 | Error interno al procesar la orden. | Error inesperado en el servicio o la persistencia |

### Cart.API

| errorCode | HTTP | Mensaje | Cuándo se devuelve |
|---|---|---|---|
| CRT-001 | 404 | Carrito no encontrado. | GET, PUT o DELETE cuando el userId no tiene un carrito activo |
| CRT-002 | 404 | Producto no encontrado. | POST o PUT cuando el ProductoId no existe en Products API |
| CRT-003 | 422 | Stock insuficiente para agregar al carrito. | POST o PUT cuando la cantidad supera el stock disponible |
| CRT-004 | 400 | Cantidad inválida. | POST o PUT cuando la cantidad es menor o igual a cero |
| CRT-005 | 500 | Error interno al procesar el carrito. | Error inesperado en el servicio o la persistencia |

### Notifications.API

| errorCode | HTTP | Mensaje | Cuándo se devuelve |
|---|---|---|---|
| NTF-001 | 404 | Usuario no encontrado. | POST cuando el UsuarioId no existe en Users API |
| NTF-002 | 400 | Los datos de la notificación son inválidos. | POST con campos faltantes o tipo no reconocido |
| NTF-003 | 404 | No se encontraron notificaciones para el usuario. | GET cuando el userId no tiene notificaciones registradas |
| NTF-004 | 500 | Error interno al procesar la notificación. | Error inesperado en el servicio o la persistencia |

Los mensajes de la tabla son los del catálogo. En algunos casos la API agrega el dato concreto,
por ejemplo `El email 'maria@email.com' ya está registrado.`.

## Cómo ejecutarlo

**Requisitos:** SDK de .NET 10.

1. Compilar toda la solución, desde la raíz del repositorio:

   ```bash
   dotnet build ECommerce.slnx
   ```

2. Levantar cada servicio en su propia terminal. Se recomienda este orden, porque algunos
   dependen de otros:

   ```bash
   dotnet run --project src/Products.API
   dotnet run --project src/Users.API
   dotnet run --project src/Cart.API
   dotnet run --project src/Notifications.API
   dotnet run --project src/Orders.API
   ```

3. Abrir el Swagger de cada servicio (ver la tabla de arriba) para probar los endpoints.

Las bases SQLite se crean solas la primera vez que arranca cada servicio, dentro de la carpeta
del proyecto.

**Notas para probar:**

- En el entorno `Development`, Products usa un Orders simulado para poder borrar productos sin
  tener Orders corriendo (`OrdersSimulado:Usar` en `appsettings.Development.json`). Para probar el
  borrado contra el Orders real (error PRD-004), poner ese valor en `false` y levantar Orders.
- Notifications llama a Users: sin Users arriba, el envío de una notificación responde un 500
  con el código NTF-004.

## Persistencia propia

La consigna menciona una librería de persistencia provista por la cátedra, pero no fue
entregada. Por eso el grupo implementó su propia persistencia: **SQLite con Dapper**, una base
por servicio, con una interfaz de repositorio por servicio (por ejemplo `IProductRepository`).
Los Services solo usan esa interfaz y ningún repositorio hace llamadas HTTP. El patrón está
explicado en `docs/persistencia.md`.

## Logs

Cada servicio escribe sus logs en la consola (formato legible) y en un archivo JSON dentro de la
carpeta `logs/` de su proyecto. Cada registro incluye Timestamp, Nivel, Servicio, Endpoint y
Correlation ID, y el `errorCode` cuando hay un error. Los errores de negocio se registran como
`Warning` y los inesperados como `Error`.

## Documentación

| Documento | Contenido |
|---|---|
| `docs/Arquitectura.md` | Arquitectura y flujos entre servicios |
| `docs/errores.md` | Contrato de error, excepciones y handlers |
| `docs/Logging.md` | Configuración de Serilog |
| `docs/CorrelationId.md` | Cómo viaja el Correlation ID entre servicios |
| `docs/plantilla/swagger.md` | Cómo está armado Swagger y cómo se agrega a un servicio |
| `docs/Servicios/` | Una guía por servicio: endpoints, archivos, errores y decisiones |
| `docs/Preguntas.md` | Preguntas probables de la defensa, con su respuesta |