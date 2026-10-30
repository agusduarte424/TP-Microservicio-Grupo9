# Logging (F2) - Serilog

## Qué resuelve
Cada servicio deja registro de lo que pasa en dos lugares: la consola (para leer mientras corre) y un archivo JSON (para buscar y procesar después).

## Dónde está
- `Extensions/LoggingExtensions.cs`
  - `AddSerilogLogging()`: configura Serilog, sus dos salidas y el campo Servicio.
  - `UseRequestLogging()`: middleware que registra el inicio y el fin de cada request.
- `Program.cs`
  - `builder.AddSerilogLogging();` va justo después de crear el builder, para registrar también los errores de arranque.
  - `app.UseRequestLogging();` es el primer middleware, para envolver a todos y ver el status final.

## Salidas
| Salida | Formato |
|---|---|
| Consola | `[HH:mm:ss NIV] Servicio CorrelationId Endpoint - Mensaje` |
| Archivo | `logs/<Servicio>-AAAAMMDD.json`: un objeto JSON por línea, un archivo por día |

La carpeta `logs/` está en el `.gitignore`: los logs no se suben al repo.

## Campos del JSON
| Campo | Quién lo completa |
|---|---|
| Timestamp, Nivel, Mensaje, Excepcion | Serilog |
| Servicio | `AddSerilogLogging()`, con el nombre del proyecto |
| Endpoint | `UseRequestLogging()`, por ejemplo `GET /api/products` |
| CorrelationId | F3 (Correlation ID) |
| ErrorCode | F4 (manejo de errores), por ejemplo `PRD-001` |

Si un campo no tiene valor, no se escribe.

## Niveles del log de fin de request
| Resultado | Nivel |
|---|---|
| 2xx | Information |
| 4xx (error del cliente o de negocio) | Warning |
| 5xx o excepción no controlada | Error |

## Ejemplo real
[12:11:09 INF] Products.API  GET /api/products - Inicio del request
[12:11:10 INF] Products.API  GET /api/products - Fin del request: 200 en 273,2 ms
[12:11:17 WRN] Products.API  GET /api/products/hola - Fin del request: 404 en 0,5 ms

## Acuerdos con el resto del grupo
- `app.UseExceptionHandler()` (F4) va **debajo** de `app.UseRequestLogging()`. Si va arriba, el log de fin no ve el 404/409 que arma el handler.
- Para sumar logging a otro servicio: agregar los paquetes `Serilog.AspNetCore` y `Serilog.Expressions`, copiar `LoggingExtensions.cs` cambiando el namespace y sumar las mismas 2 líneas en `Program.cs`.