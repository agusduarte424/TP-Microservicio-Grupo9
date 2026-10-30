# Cart.API

Puerto: **5004**. Base de datos propia: `cart.db` (SQLite + Dapper).
Depende de: **Products.API (5001)** para validar que el producto exista y tenga stock.

## Endpoints
| Método | Ruta | Qué hace | OK |
|---|---|---|---|
| GET | /api/cart/{userId} | Ver el carrito | 200 |
| POST | /api/cart/{userId}/items | Agregar producto (si ya está, suma) | 200 |
| PUT | /api/cart/{userId}/items/{productId} | Reemplazar la cantidad | 200 |
| DELETE | /api/cart/{userId}/items/{productId} | Quitar un producto | 204 |
| DELETE | /api/cart/{userId} | Vaciar el carrito | 204 |

## Errores
| Código | HTTP | Cuándo |
|---|---|---|
| CRT-001 | 404 | El usuario no tiene carrito |
| CRT-002 | 404 | El producto no existe o no está en el carrito |
| CRT-003 | 422 | No hay stock suficiente |
| CRT-004 | 400 | Cantidad menor o igual a 0 |
| CRT-005 | 500 | Error interno o Products no responde |

## Preguntas de defensa
- **¿Por qué Cart le pregunta a Products?** Porque cada servicio tiene su propia base y Cart no puede leer la de Products.
- **¿Por qué se valida el stock contra el total?** Porque si ya tenés 3 y agregás 2, lo que importa es que haya 5.
- **¿Qué pasa si Products está caído?** Cart responde CRT-005, no inventa datos.
- **¿Cómo se sigue un pedido entre servicios?** Con el Correlation ID: el header `X-Correlation-Id` pasa de Cart a Products y aparece en los logs de los dos.
- **¿Por qué el alias `CartModel`?** Porque la clase `Cart` choca con el namespace `Cart.API`.