# Correlation ID (F3)

## Qué resuelve
Cada request lleva un id (`X-Correlation-Id`) que viaja entre servicios. Si una compra pasa por Orders, Products y Users, el mismo id aparece en los logs de los tres, y buscándolo se ve el recorrido completo.

## Dónde está
- `Extensions/CorrelationIdExtensions.cs`
  - `AddCorrelationId()`: registra `CorrelationIdHandler` en todos los HttpClient del servicio.
  - `UseCorrelationId()`: middleware que toma el id del header o genera uno nuevo.
  - `GetCorrelationId()`: devuelve el id del request actual.
  - `CorrelationIdHandler`: agrega el header en cada llamada HTTP saliente.
- `Program.cs`
  - `builder.Services.AddCorrelationId();` después de `AddSerilogLogging()`.
  - `app.UseCorrelationId();` antes de `app.UseRequestLogging()`, para que "Inicio del request" ya lleve el id.

## Cómo funciona
1. Llega un request. Si trae `X-Correlation-Id`, se usa ese; si no, se genera un Guid.
2. El id se guarda en `HttpContext.Items`, se agrega a todos los logs (LogContext) y se devuelve en el header de la respuesta.
3. Si el servicio llama a otro, `CorrelationIdHandler` le manda el mismo header.
4. El otro servicio lo recibe en el paso 1 y lo respeta.

## Cómo probarlo
1. `(Invoke-WebRequest http://localhost:5001/api/products -UseBasicParsing).Headers["X-Correlation-Id"]` devuelve un Guid.
2. Mandando `-Headers @{"X-Correlation-Id"="prueba-1"}`, la respuesta y los logs muestran `prueba-1`.
3. Propagación: con Products y Orders corriendo, un DELETE de producto con un id conocido muestra ese mismo id en los logs de Orders (`GET /api/orders`).

## Acuerdos con el resto del grupo
- F4: las respuestas de error incluyen el campo `correlationId`, que se obtiene con `httpContext.GetCorrelationId()`.
- Para sumarlo a otro servicio: copiar `CorrelationIdExtensions.cs` cambiando el namespace y agregar las mismas 2 líneas en `Program.cs`. Todo HttpClient registrado con `AddHttpClient` propaga el id sin más cambios.