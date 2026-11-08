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
