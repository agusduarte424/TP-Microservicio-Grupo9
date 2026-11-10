# Orders.API

Puerto: http://localhost:5003
Base: orders.db (SQLite, se crea sola al arrancar)

## Propósito

Maneja las órdenes de compra: crearlas, consultarlas y cambiarles el estado.
Al crear una orden valida al usuario contra Users.API y los productos, el stock y el precio contra Products.API.

## Endpoints

| Método | Ruta | Qué hace | Status posibles |
|---|---|---|---|
| GET | /api/orders?usuarioId= | Lista las órdenes, con filtro opcional por usuario | 200, 400 (ORD-002), 500 (ORD-007) |
| GET | /api/orders/{id} | Detalle de una orden | 200, 404 (ORD-001), 500 (ORD-007) |
| POST | /api/orders | Crea una orden en estado Pendiente | 201, 400 (ORD-002), 404 (ORD-003, ORD-004), 422 (ORD-005), 500 (ORD-007) |
| PUT | /api/orders/{id}/status | Cambia el estado | 200, 400 (ORD-002), 404 (ORD-001), 409 (ORD-006), 500 (ORD-007) |

## Recorrido de una request

Ejemplo: `POST /api/orders`.

1. `Program.cs` arma la cadena: Correlation ID, logging de requests, `UseExceptionHandler`, Swagger, controladores.
2. `OrdersController.Create` recibe el body. Si falta un campo `[Required]` o el JSON está mal formado, responde 400 (ORD-002) desde `ErrorHandlingExtensions.cs`.
3. `OrderService.Crear` valida los datos y junta todos los problemas en un solo mensaje (ORD-002). Si un producto viene repetido, suma las cantidades.
4. `UsersClient.Existe` hace `GET api/users/{id}`. Un 404 es ORD-003; cualquier otra falla lanza excepción (ORD-007).
5. Por cada producto, `ProductsClient.ObtenerProducto` hace `GET api/products/{id}`. Un 404 es ORD-004. Si la cantidad supera el stock, ORD-005.
6. El Service copia el precio actual de cada producto, calcula el total y arma la orden en estado Pendiente.
7. `OrderRepository.Crear` guarda la orden y sus items en una sola transacción.
8. El controlador devuelve 201 con la orden y el header `Location`.

## Estados y transiciones

La consigna nombra los estados pero no define los cambios permitidos. Se siguió el ciclo real de un pedido:


```
Pendiente ──► Confirmada ──► Enviada ──► Entregada
    │              │
    └──► Cancelada ◄┘
```

- Se cancela solo lo que todavía no se envió.
- Entregada y Cancelada son finales.
- Cualquier otro cambio responde 409 con ORD-006. Las reglas están en `Models/EstadoOrden.cs`.

## Archivos clave

| Archivo | Qué hace |
|---|---|
| `Controllers/OrdersController.cs` | Los 4 endpoints |
| `Services/OrderService.cs` | Validaciones, cálculo del total y cambio de estado |
| `Services/IUsersClient.cs` y `UsersClient.cs` | Consulta si el usuario existe en Users.API |
| `Services/IProductsClient.cs` y `ProductsClient.cs` | Consulta producto, precio y stock en Products.API |
| `Data/IOrderRepository.cs` y `OrderRepository.cs` | Persistencia con SQLite y Dapper; guardado en transacción |
| `Data/DatabaseInitializer.cs` | Crea las tablas `orders` y `order_items` al arrancar |
| `Models/EstadoOrden.cs` | Los 5 estados y las transiciones permitidas |
| `Exceptions/ErrorCodes.cs` | Constantes ORD-001 a ORD-007 |
| `ExceptionHandlers/*.cs` | Un handler para 404, 400, 422, 409 y el genérico 500 |

## Llamadas a otros servicios

| Llama a | Para qué | Si falla |
|---|---|---|
| Users.API (`GET /api/users/{id}`) | Validar que el usuario exista | 404: ORD-003. Caída, timeout o 5xx: ORD-007 |
| Products.API (`GET /api/products/{id}`) | Validar producto, stock y obtener el precio | 404: ORD-004. Caída, timeout o 5xx: ORD-007 |

- Las URLs salen de `appsettings.json` (`Services:UsersApi` y `Services:ProductsApi`) y el timeout es de 5 segundos.
- Products.API también llama a Orders (`GET /api/orders`) antes de borrar un producto, para verificar que no tenga órdenes Pendiente o Confirmada (PRD-004).
- El Correlation ID viaja en el header `X-Correlation-Id` y aparece en los logs de Orders, Users y Products.

## Decisiones

- **El precio se guarda en cada item:** si después cambia en Products, la orden mantiene lo que se cobró.
- **ORD-005 es 422 y ORD-006 es 409:** el stock insuficiente es una regla de negocio que impide procesar la orden; la transición inválida choca con el estado actual del recurso. Por eso hay una `ConflictException` además de `BusinessRuleException`.
- **El stock no se descuenta:** la consigna pide validarlo, no reservarlo.

## Cómo probar

Con Users, Products y Orders corriendo, usar `Orders.API.http` o Swagger en http://localhost:5003/swagger.