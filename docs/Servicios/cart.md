# Cart.API

Puerto: http://localhost:5004
Base: cart.db (SQLite, se crea sola al arrancar)

## Propósito

Maneja el carrito de compras de cada usuario: agregar, cambiar y quitar productos.
Valida los productos y el stock contra Products.API. **No valida al usuario**: el carrito se
identifica por el `userId` de la ruta y la consigna no pide chequearlo contra Users.

## Endpoints

| Método | Ruta | Qué hace | Status posibles |
|---|---|---|---|
| GET | /api/cart/{userId} | Ver el carrito | 200, 404 (CRT-001), 500 (CRT-005) |
| POST | /api/cart/{userId}/items | Agregar un producto (si ya está, suma la cantidad). Si el usuario no tiene carrito, lo crea | 200, 400 (CRT-004), 404 (CRT-002), 422 (CRT-003), 500 (CRT-005) |
| PUT | /api/cart/{userId}/items/{productId} | Reemplazar la cantidad de un producto del carrito | 200, 400 (CRT-004), 404 (CRT-001, CRT-002), 422 (CRT-003), 500 (CRT-005) |
| DELETE | /api/cart/{userId}/items/{productId} | Quitar un producto | 204, 404 (CRT-001, CRT-002), 500 (CRT-005) |
| DELETE | /api/cart/{userId} | Vaciar el carrito | 204, 404 (CRT-001), 500 (CRT-005) |

## Recorrido de una request

Ejemplo: `POST /api/cart/{userId}/items`.

1. `Program.cs` arma la cadena: Correlation ID, logging de requests, `UseExceptionHandler`, Swagger, controladores.
2. `CartController.AddItem` recibe el body. Si falta el `productoId` o el JSON está mal formado, responde un 400 (CRT-004) desde `ErrorHandlingExtensions.cs` sin llegar al Service.
3. `CartService.AgregarItem` valida que la cantidad sea mayor a 0 (CRT-004) y le pregunta a `IProductsClient.ObtenerProducto` si el producto existe.
4. `ProductsClient` hace `GET api/products/{id}` a Products.API con el mismo `X-Correlation-Id`. Un 404 devuelve `null` (CRT-002); otro código que no sea 2xx lanza una excepción (CRT-005).
5. El Service busca el carrito del usuario (o arma uno nuevo), suma la cantidad si el producto ya estaba y compara el **total** contra el stock (CRT-003).
6. Actualiza `FechaActualizacion` y guarda con `ICartRepository.Guardar`. `CartRepository` ejecuta el SQL con Dapper contra `cart.db`.
7. El controlador devuelve 200 con `CartResponse`. Si algo falló, el handler correspondiente arma el JSON de error con `errorCode`, `errorMessage` y `correlationId`.

## Archivos clave

| Archivo | Qué hace | Por qué existe |
|---|---|---|
| `Controllers/CartController.cs` | Los 5 endpoints | Recibe el pedido HTTP y devuelve el status |
| `Services/CartService.cs` | Reglas del carrito: validar cantidad, sumar, comparar stock | Concentra la lógica de negocio |
| `Services/IProductsClient.cs` y `ProductsClient.cs` | Consulta un producto a Products.API | El Service no depende de cómo se hace la llamada HTTP |
| `Data/ICartRepository.cs` y `CartRepository.cs` | Persistencia con SQLite y Dapper | El Service solo conoce la interfaz, y el repositorio no hace llamadas HTTP |
| `Data/DatabaseInitializer.cs` | Crea las tablas al arrancar | La base se arma sola |
| `Exceptions/ErrorCodes.cs` | Constantes CRT-001 a CRT-005 | Evita strings sueltos |
| `ExceptionHandlers/*.cs` | Un handler para 404, 400, 422 y el genérico 500 | Respuestas con el contrato de la consigna |
| `DTOs/*.cs` | Request y response de cada endpoint | Separan lo que entra y sale del modelo |
| `Extensions/*.cs` | Logging, Correlation ID, errores, persistencia y Swagger | Cada bloque transversal en su archivo |

## Llamadas a otros servicios

| Llama a | Para qué | Si falla |
|---|---|---|
| Products.API (`GET /api/products/{id}`) | Validar que el producto exista y obtener su stock, en POST y PUT | 404 de Products: CRT-002 (404). Timeout, caída o 5xx: CRT-005 (500) y no se guarda nada |

- La URL de Products sale de `appsettings.json` (`Services:ProductsApi`) y el timeout es de 5 segundos.
- `GET` y los dos `DELETE` **no** llaman a Products: se resuelven con la base local.
- El Correlation ID viaja en el header `X-Correlation-Id`, y aparece en los logs de Cart y de Products.

## Errores

| errorCode | HTTP | Cuándo se lanza | Dónde |
|---|---|---|---|
| CRT-001 | 404 | El usuario no tiene carrito (GET, PUT, DELETE) | `CartService.BuscarOFallar` |
| CRT-002 | 404 | El producto no existe en Products API (POST, PUT), o no está en el carrito (PUT, DELETE de un item) | `CartService.ObtenerProductoOFallar`, `ActualizarItem` y `QuitarItem` |
| CRT-003 | 422 | La cantidad (o el total que quedaría en el carrito) supera el stock | `CartService.AgregarItem` y `ActualizarItem` |
| CRT-004 | 400 | Cantidad menor o igual a 0, `productoId` faltante o JSON malformado | `CartService.ValidarCantidad`; los otros dos en `InvalidModelStateResponseFactory` |
| CRT-005 | 500 | Error inesperado, incluida una falla al llamar a Products | `GlobalExceptionHandler` |

## Decisiones de diseño

- **Cart no valida al usuario.** Un usuario sin carrito responde CRT-001 en GET, PUT y DELETE. El POST crea el carrito si no existe. En G17 el carrito validaba al usuario en todos los endpoints y devolvía un código que no existe en el catálogo.
- **El stock se compara contra el total.** Si ya tenés 3 unidades y agregás 2, lo que importa es que haya 5 disponibles. Por eso el "solicitado" del mensaje puede ser mayor que lo que mandaste en ese request.
- **Si el producto ya está en el carrito, se suman las cantidades** en el POST. El PUT, en cambio, reemplaza la cantidad.
- **`FechaActualizacion` se actualiza en cada operación** que modifica el carrito.
- **La cantidad se valida antes de consultar a Products**, para no hacer una llamada HTTP si el dato ya es inválido.
- **CRT-002 también cubre "el producto no está en el carrito"** (PUT y DELETE de un item). El catálogo no tiene otro código 404 para ese caso, y no se inventan códigos.
- **El 400 de validación del body usa CRT-004**, que es el único 400 del catálogo de Cart.
- **Vaciar el carrito** deja el carrito existiendo con la lista de items vacía: un GET posterior responde 200 y no 404.
- **Un 404 de Products es CRT-002 y cualquier otro fallo es CRT-005**: si no podemos consultar a Products, no sabemos si el producto existe, así que no se le dice al cliente que no existe.
- **El alias `CartModel`**: la clase `Cart` choca con el namespace `Cart.API`.
- **El `BusinessRuleException` de Cart responde 422** (no 409 como en Products y Users), porque el único error de negocio del catálogo, CRT-003, es un 422.

## Cómo probarlo

Con `Cart.API.http` (en la carpeta del servicio) o desde Swagger en http://localhost:5004/swagger. Products.API tiene que estar corriendo en el puerto 5001 y tener al menos un producto.

- **Caso exitoso:** `POST /api/cart/{userId}/items` con un `productoId` real y `cantidad` 1 responde 200 con el carrito.
- **Caso de error:** el mismo POST con `cantidad` 9999 responde 422 con CRT-003.
- **Otros:** `GET` de un usuario sin carrito da 404 con CRT-001, `cantidad` 0 da 400 con CRT-004, y un `productoId` inexistente da 404 con CRT-002.
- **Falla de Products:** con Products apagado, un POST válido responde 500 con CRT-005 y no se guarda nada.

## Preguntas de defensa

1. ¿Por qué Cart le pregunta a Products?
   Porque cada servicio tiene su propia base y Cart no puede leer la de Products.
2. ¿Por qué se valida el stock contra el total?
   Porque si ya tenés 3 y agregás 2, lo que importa es que haya 5.
3. ¿Qué pasa si Products está caído?
   Cart responde 500 con CRT-005 y no guarda nada. Si Products responde 404, es CRT-002. Un 404 es una respuesta de negocio; un timeout o un 5xx no nos dice si el producto existe, así que no inventa datos.
4. ¿Cómo se sigue un pedido entre servicios?
   Con el Correlation ID: el header `X-Correlation-Id` pasa de Cart a Products y aparece en los logs de los dos.
5. ¿Por qué Cart no valida que el usuario exista?
   Porque la consigna solo le pide validar productos y stock contra Products. Un usuario sin carrito se trata como CRT-001, y el POST lo crea.
6. ¿Por qué CRT-003 es 422 y no 400?
   El request está bien formado, así que no es un 400. Lo que falla es una regla de negocio (no hay stock), y eso es un 422.
7. ¿Por qué el alias `CartModel`?
   Porque la clase `Cart` choca con el namespace `Cart.API`.