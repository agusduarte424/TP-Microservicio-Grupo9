# Products.API

Puerto: http://localhost:5001
Base: products.db (SQLite, se crea sola al arrancar)

## Endpoints
| Método | Ruta | Respuestas |
|---|---|---|
| GET | /api/products?categoria=&nombre= | 200 |
| GET | /api/products/{id} | 200, 404 (PRD-001) |
| POST | /api/products | 201, 400 (PRD-002), 409 (PRD-003) |
| PUT | /api/products/{id} | 200, 400 (PRD-002), 404 (PRD-001) |
| DELETE | /api/products/{id} | 204, 404 (PRD-001), 409 (PRD-004), 500 (PRD-005) |

## Capas
Controller -> Service -> Repository -> SQLite
- Controller: recibe el pedido HTTP y devuelve el código.
- Service: reglas de negocio (duplicados, órdenes activas) y excepciones PRD.
- Repository: SQL con Dapper, siempre con parámetros.

## Comunicación con Orders
Antes de borrar, Products le pregunta a Orders (GET /api/orders) si el producto
está en órdenes Pendiente o Confirmada. Mientras Orders no exista, se usa un
Orders simulado (OrdersSimulado:Usar = true en appsettings.Development.json).

## Preguntas de defensa
1. ¿Por qué el DELETE consulta a Orders antes de borrar?
   Para no dejar órdenes activas apuntando a un producto que ya no existe.
   Si Orders no responde, no se borra (PRD-005).
2. ¿Por qué el Service usa interfaces (IProductRepository, IOrdersClient)?
   Para no depender de una implementación concreta: se puede cambiar
   SQLite por otra base, o el Orders simulado por el real, sin tocar el Service.
3. ¿Por qué los parámetros @Nombre, @Id en el SQL?
   Para evitar inyección SQL: el valor nunca se pega en el texto de la consulta.
4. ¿Por qué Precio y Stock son decimal? e int? en los DTOs?
   Para que [Required] detecte cuando no vienen. Sin el ?, llegarían como 0.