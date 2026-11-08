# Swagger (bloque transversal F6)

Cómo está armada la documentación de la API en cada servicio y cómo agregarla a uno nuevo.
Products.API es la plantilla: los otros servicios copian este bloque y cambian solo el título
y los códigos de error.

## Qué ofrece

- Swagger UI en `/swagger` de cada servicio, disponible en todos los entornos.
- Descripciones tomadas de los XML comments (`/// <summary>`) de controladores y modelos.
- Endpoints agrupados por controlador (un tag por recurso).
- Todos los códigos de respuesta de la consigna en cada endpoint, con un ejemplo de error
  que muestra `errorCode` y `errorMessage`.
- Ejemplos de éxito en los modelos, tomados de los ejemplos de la consigna.

## Archivos

| Archivo | Qué hace |
|---|---|
| `Extensions/SwaggerExtensions.cs` | `AddSwaggerDocs()` registra el generador, lee los XML comments y el filtro; `UseSwaggerDocs()` publica `/swagger` |
| `Extensions/ErrorExampleAttribute.cs` | Atributo `[ErrorExample]` para declarar un error posible de un endpoint |
| `Extensions/ErrorExamplesOperationFilter.cs` | Convierte cada `[ErrorExample]` en un ejemplo con el contrato de error de la consigna |
| `Products.API.csproj` | `GenerateDocumentationFile` (genera el `.xml` con los comentarios) y el paquete Swashbuckle 6.6.2 |

## Cómo funciona

1. El compilador guarda los `/// <summary>` y `/// <example>` en un archivo `.xml` (por
   `GenerateDocumentationFile`).
2. `AddSwaggerGen` con `IncludeXmlComments` se los pasa a Swagger, que los muestra como
   descripciones y valores de ejemplo.
3. Para los errores, el controlador declara `[ProducesResponseType]` (el status) y
   `[ErrorExample]` (el código y mensaje del catálogo).
4. `ErrorExamplesOperationFilter` lee los `[ErrorExample]` al generar la documentación y arma el
   ejemplo con los 8 campos que devuelven los handlers: `type`, `title`, `status`, `detail`,
   `instance`, `errorCode`, `errorMessage` y `correlationId`.

El ejemplo sale de un texto fijo, no de la respuesta real. Por eso, al documentar un endpoint,
hay que compararlo con lo que devuelve la API.

## Cómo documentar un endpoint

```csharp
/// <summary>Elimina un producto si no tiene órdenes Pendiente o Confirmada.</summary>
[HttpDelete("{id:guid}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[ErrorExample(404, ErrorCodes.ProductoNoEncontrado, "Producto no encontrado.")]
[ErrorExample(409, ErrorCodes.ProductoConOrdenesActivas, "El producto tiene órdenes activas y no puede eliminarse.",
    Detail = "No se puede eliminar el recurso.")]
[ErrorExample(500, ErrorCodes.ErrorInterno, "Error interno al procesar el producto.")]
public async Task<IActionResult> Delete(Guid id) { ... }
```

- Un `[ErrorExample]` por cada error del catálogo que el endpoint puede devolver.
- Si un mismo status tiene dos errores (por ejemplo dos 403 en el login), se declaran los dos
  atributos y Swagger los muestra como dos ejemplos para elegir.
- `Detail` solo hace falta cuando el `detail` de la consigna no es el genérico de ese status.
- Los ejemplos de éxito se escriben en el modelo: `/// <example>Notebook Dell XPS 15</example>`
  debajo del `/// <summary>` de cada propiedad.
- Siempre se usan códigos y mensajes del catálogo de la consigna.

## Cómo agregarlo a otro servicio

1. En el `.csproj`: agregar `GenerateDocumentationFile` y `NoWarn` 1591 dentro del
   `PropertyGroup`, **quitar** `Microsoft.AspNetCore.OpenApi` y agregar
   `Swashbuckle.AspNetCore` 6.6.2.
2. Copiar a `Extensions/` los tres archivos de arriba, cambiando el namespace por el del
   servicio. En `SwaggerExtensions.cs` cambiar también el título y la descripción.
3. En `Program.cs`: `builder.Services.AddSwaggerDocs();` debajo de `AddErrorHandling()` y
   `app.UseSwaggerDocs();` debajo de `app.UseExceptionHandler();`.
4. En cada controlador: los `using` de `Exceptions` y `Extensions`, y los
   `[ProducesResponseType]` y `[ErrorExample]` con los status de la consigna.
5. En los modelos de entrada y salida: los `/// <example>` de los ejemplos de éxito.

## Problemas conocidos

| Síntoma | Causa | Qué hacer |
|---|---|---|
| Errores de compilación o de ejecución sobre `Microsoft.OpenApi` | Conviven `Microsoft.AspNetCore.OpenApi` 10.x (usa `Microsoft.OpenApi` 2.x) y Swashbuckle 6.x (usa 1.x) | Quitar `Microsoft.AspNetCore.OpenApi` del `.csproj` |
| El código de los filtros no compila al subir Swashbuckle | La 10.x usa `Microsoft.OpenApi` 2.x, con una API distinta | No actualizar Swashbuckle sin avisar al grupo |
| Un endpoint no muestra los ejemplos de error | Falta el `[ErrorExample]` o el `[ProducesResponseType]` de ese status | Declarar los dos atributos |
| Las descripciones no aparecen | El `.xml` no se genera o `IncludeXmlComments` no lo encuentra | Revisar `GenerateDocumentationFile` en el `.csproj` |

## Decisiones de diseño

- **Swashbuckle 6.6.2 y no una versión nueva:** es la versión del plan, y las versiones 10.x
  cambian la API de los filtros. Se descartó actualizar para no romper el código ni la
  compatibilidad con el resto del grupo.
- **Un atributo y un filtro en vez de escribir el JSON de cada error:** el contrato de error
  tiene 8 campos y se repite en decenas de endpoints, así que hay una sola definición.
- **Swagger en todos los entornos:** la consigna lo pide en cada servicio y la defensa se hace
  sobre él.
- **Se descartó `Microsoft.AspNetCore.OpenApi`:** la consigna pide Swashbuckle y no conviven.

## Preguntas de defensa

1. ¿De dónde saca Swagger las descripciones de los endpoints?
   De los `/// <summary>` del código. `GenerateDocumentationFile` hace que el compilador los guarde
   en un `.xml` y `IncludeXmlComments` se lo pasa a Swagger.
2. ¿Cómo se documenta un error en un endpoint?
   Con `[ProducesResponseType]` para el status y `[ErrorExample]` para el código y el mensaje. Un
   filtro de Swagger convierte el atributo en un ejemplo con el contrato de error de la consigna.
3. ¿Por qué se usa Swashbuckle 6.6.2 y no la versión más nueva?
   Porque es la versión del plan y las 10.x cambian la API de `Microsoft.OpenApi`, lo que rompe
   los filtros. Además no puede convivir con `Microsoft.AspNetCore.OpenApi`, por eso se quitó.
4. ¿El ejemplo que muestra Swagger es la respuesta real de la API?
   No: es un texto fijo que escribimos. Por eso se compara con la respuesta real cuando se
   documenta cada endpoint.
