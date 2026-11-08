# Manejo de errores

Cómo se manejan los errores en los servicios. Products.API es la plantilla: los otros
servicios copian esta estructura y cambian solo el catálogo de códigos.

## Idea general

Ningún controlador tiene `try/catch`. Cuando algo sale mal, el **Service lanza una excepción
de dominio** con un código del catálogo, y ASP.NET Core se la pasa a los `IExceptionHandler`,
que arman la respuesta con el contrato de la consigna (sección 3.1).

```
Controller → Service lanza excepción → UseExceptionHandler → handler → JSON de error
```

## Contrato de la respuesta de error

Todo error (4xx y 5xx) tiene esta forma:

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

`correlationId` es un campo extra (sección 5.5 de la consigna): es el mismo valor que viaja en
el header `X-Correlation-Id` y que aparece en los logs, así un error se puede rastrear.

Para errores de validación con varios campos, `errorMessage` lista todos los problemas
separados por punto y coma.

## Catálogo de Products

Las constantes están en `Exceptions/ErrorCodes.cs`, para no escribir strings sueltos.

| errorCode | HTTP | Cuándo se lanza | Dónde |
|---|---|---|---|
| PRD-001 | 404 | GET, PUT o DELETE con un id que no existe | `ProductService.BuscarOFallar` |
| PRD-002 | 400 | POST o PUT con campos faltantes o inválidos | `InvalidModelStateResponseFactory` en `ErrorHandlingExtensions.cs` |
| PRD-003 | 409 | POST con un nombre que ya existe en esa categoría | `ProductService.Crear` |
| PRD-004 | 409 | DELETE de un producto con órdenes Pendiente o Confirmada | `ProductService.Eliminar` |
| PRD-005 | 500 | Cualquier error inesperado, incluida una falla al consultar Orders | `GlobalExceptionHandler` |

## Excepciones de dominio

Están en `Exceptions/`. Todas llevan un `ErrorCode` y el mensaje (`errorMessage`).

| Excepción | Representa | Status |
|---|---|---|
| `NotFoundException` | Un recurso que no existe | 404 |
| `ValidationException` | Datos inválidos detectados en el Service | 400 |
| `BusinessRuleException` | Una regla de negocio violada (duplicado, producto con órdenes activas) | 409 |

## Handlers

Están en `ExceptionHandlers/`. Cada uno reconoce **un** tipo de excepción: si no es de su tipo
devuelve `false` y ASP.NET prueba con el siguiente.

| Handler | Atrapa | Responde |
|---|---|---|
| `NotFoundExceptionHandler` | `NotFoundException` | 404 |
| `ValidationExceptionHandler` | `ValidationException` | 400 |
| `BusinessRuleExceptionHandler` | `BusinessRuleException` | 409 |
| `GlobalExceptionHandler` | Cualquier otra excepción | 500 con PRD-005 |

`ErrorResponseWriter` es el único lugar que escribe el JSON de error, así que los cuatro
handlers responden con exactamente la misma estructura.

### Por qué importa el orden

Se registran en `Extensions/ErrorHandlingExtensions.cs` y ASP.NET los recorre en el orden en
que se registran, usando el primero que devuelve `true`. Por eso **los específicos van antes y
el genérico va último**: si el genérico fuera primero, atraparía todo y los 404 y 409 saldrían
como 500.

## Errores de validación del body (PRD-002)

Los DTOs usan Data Annotations (`[Required]`, `[StringLength]`, `[Range]`). Cuando un body no
las cumple, el framework corta **antes de llegar al Service**, así que no se lanza ninguna
excepción nuestra y no pasa por los handlers.

Por eso el 400 se arma en el `InvalidModelStateResponseFactory` configurado en
`ErrorHandlingExtensions.cs`. Hace tres cosas:

1. Junta todos los mensajes de error en un solo texto separado por `; `.
2. Registra un `Warning` con el `errorCode` PRD-002.
3. Devuelve el mismo contrato de error, con `correlationId`.

Un JSON malformado entra por el mismo camino. En ese caso el framework no trae mensaje y se
usa uno de reemplazo: "El cuerpo de la solicitud no es un JSON válido."

## Detalle por entorno

Nunca se devuelve un stack trace. Lo único que cambia entre entornos es el campo `detail` del
error 500, en `GlobalExceptionHandler`:

| Entorno | `detail` del 500 |
|---|---|
| Development | El mensaje real de la excepción, para depurar |
| Production | "Ocurrió un error inesperado." |

El stack trace completo queda solo en el log del servidor, nunca en la respuesta.

## Cómo se registra en los logs

| Tipo de error | Nivel | Dónde se escribe |
|---|---|---|
| De negocio (404, 400, 409) | `Warning`, con el `errorCode` | Cada handler |
| Inesperado (500) | `Error`, con la excepción y el `errorCode` | `GlobalExceptionHandler` |

## Fallas al llamar a otro servicio

Si Products llama a Orders (en el DELETE) y Orders responde con un error o no responde, la
llamada lanza una excepción. Nadie la captura, llega a `GlobalExceptionHandler` y el resultado
es un 500 con PRD-005. El producto **no se borra**.

## Cómo agregar un código nuevo

1. Sumar la constante en `Exceptions/ErrorCodes.cs`, con un código del catálogo de la consigna.
2. Lanzar la excepción que corresponda desde el Service.
3. Si es un caso de status nuevo (por ejemplo 401 o 403), crear la excepción y su handler, y
   registrar el handler **antes** del genérico.

## Preguntas de defensa

1. **¿Por qué `IExceptionHandler` y no un middleware propio?**
   Porque es el mecanismo que trae ASP.NET Core para esto y la consigna prohíbe el middleware
   propio. Cada handler hace una sola cosa y se registra con `AddExceptionHandler`.
2. **¿Por qué el handler genérico va último?**
   Los handlers se prueban en orden y se usa el primero que devuelve `true`. El genérico
   atrapa cualquier excepción, así que si estuviera primero convertiría los 404 y 409 en 500.
3. **¿Por qué el 400 de validación no es un `IExceptionHandler`?**
   Porque cuando falla una Data Annotation el framework corta antes de llegar al Service y no
   lanza ninguna excepción, así que no hay nada que un handler pueda atrapar. Se configura en
   `InvalidModelStateResponseFactory`.
